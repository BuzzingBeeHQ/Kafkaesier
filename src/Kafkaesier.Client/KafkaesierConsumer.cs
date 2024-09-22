using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client;

public class KafkaesierConsumer<TCommand, THandler>(IServiceProvider serviceProvider, IOptions<KafkaClientOptions> options) : IKafkaesierConsumer
    where TCommand : CommandBase
    where THandler : CommandHandlerBase<TCommand>
{
    private readonly KafkaClientOptions _kafkaClientOptions = options.Value;
    private readonly IConsumer<Ignore, string> _kafkaConsumer = CreateConsumer(options.Value);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string topicName;

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var kafkaesierAdminClient = scope.ServiceProvider.GetRequiredService<IKafkaesierAdminClient>();
            topicName = await kafkaesierAdminClient.CreateTopicOrSkipAsync<TCommand>();
        }

        _kafkaConsumer.Subscribe(topicName);
        await ExecuteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Close();
        _kafkaConsumer.Dispose();
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var consumeResult = _kafkaConsumer.Consume(_kafkaClientOptions.ConsumerBlockingTimeoutInMilliseconds);

            var command = JsonSerializer.Deserialize<TCommand>(consumeResult.Message.Value);
            ArgumentNullException.ThrowIfNull(command);

            await HandleCommandInScopeAsync(command);

            _kafkaConsumer.Commit(consumeResult);
        }
    }

    private async Task HandleCommandInScopeAsync(TCommand command)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handlerInstance = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider);
        await handlerInstance.StartHandleAsync(command);
    }

    private static IConsumer<Ignore, string> CreateConsumer(KafkaClientOptions options)
    {
        var groupId = KafkaesierNameBuilder.CreateConsumerGroupName<THandler>()
            .WithPrefix(options.NamePrefix)
            .Build();

        var optionsDictionary = options.AsDictionary(OptionTargets.Consumer);
        optionsDictionary[OptionNames.GroupId] = groupId;
        return new ConsumerBuilder<Ignore, string>(optionsDictionary).Build();
    }
}