using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client;

public class KafkaesierConsumer<TCommand, THandler>(IServiceProvider serviceProvider, IOptions<KafkaClientOptions> options) : BackgroundService
    where TCommand : CommandBase
    where THandler : CommandHandlerBase<TCommand>
{
    private readonly KafkaClientOptions _kafkaClientOptions = options.Value;
    private readonly IConsumer<Ignore, string> _kafkaConsumer = CreateConsumer(options.Value);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        string topicName;

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var kafkaesierAdminClient = scope.ServiceProvider.GetRequiredService<IKafkaesierAdminClient>();
            topicName = await kafkaesierAdminClient.CreateTopicOrSkipAsync<TCommand>();
        }

        _kafkaConsumer.Subscribe(topicName);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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
 
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Close();
        _kafkaConsumer.Dispose();
        return base.StopAsync(cancellationToken);
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