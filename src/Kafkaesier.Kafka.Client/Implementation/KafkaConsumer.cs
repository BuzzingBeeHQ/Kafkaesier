using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Constants;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Kafka.Client.Implementation;

public class KafkaConsumer<TMessage, THandler>(IServiceProvider serviceProvider, IOptions<KafkaClientOptions> options) : IKafkaesierConsumer
    where TMessage : MessageBase
    where THandler : CommandHandlerBase<TMessage>
{
    private readonly KafkaClientOptions _options = options.Value;
    private readonly IConsumer<string, string> _consumer = CreateConsumer(options.Value);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string topicName;

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var adminClient = scope.ServiceProvider.GetRequiredKeyedService<IKafkaesierAdminClient>(nameof(KafkaAdminClient));
            topicName = await adminClient.CreateTopicOrSkipAsync<TMessage>();
        }

        _consumer.Subscribe(topicName);
        _ = Task.Run(async () => await ExecuteAsync(cancellationToken), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(_options.ConsumerBlockingTimeoutInMilliseconds);

            var message = JsonSerializer.Deserialize<TMessage>(consumeResult.Message.Value)!;
            await HandleMessageInScopeAsync(message);

            _consumer.Commit(consumeResult);
        }
    }

    private async Task HandleMessageInScopeAsync(TMessage message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handlerInstance = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider);
        await handlerInstance.StartHandleAsync(message);
    }

    private static IConsumer<string, string> CreateConsumer(KafkaClientOptions options)
    {
        var groupId = KafkaesierNameBuilder.CreateConsumerGroupName<THandler, TMessage>()
            .WithPrefix(options.Prefix)
            .Build();

        var optionsDictionary = options.AsDictionary(KafkaOptionTargets.Consumer);
        optionsDictionary[OptionNames.GroupId] = groupId;
        return new ConsumerBuilder<string, string>(optionsDictionary).Build();
    }
}