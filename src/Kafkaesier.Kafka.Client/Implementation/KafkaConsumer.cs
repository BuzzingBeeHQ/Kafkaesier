using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Constants;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Kafka.Client.Implementation;

public class KafkaConsumer<TMessage, THandler>(
    IServiceProvider serviceProvider,
    ILogger<IKafkaesierConsumer> logger,
    IOptions<KafkaClientOptions> options)
    : IKafkaesierConsumer
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

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, $"Unhandled exception of type: {exception.GetType().Name} in {typeof(KafkaConsumer<TMessage, THandler>)}. Details: {exception.Message}.");
                }
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        _consumer.Dispose();
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync()
    {
        var consumeResult = _consumer.Consume(_options.ConsumerBlockingTimeoutInMilliseconds);
        var message = CreateMessageFromJson<TMessage>(consumeResult.Message.Value);

        await HandleMessageInScopeAsync(message);
        _consumer.Commit(consumeResult);
    }

    private T CreateMessageFromJson<T>(string jsonData)
    {
        try
        {
            var message = JsonSerializer.Deserialize<T>(jsonData) ?? throw new ArgumentException("JSON data was deserialized to null.", nameof(jsonData));

            logger.LogDebug($"Received message of type {typeof(T)} with payload: {jsonData} in {typeof(KafkaConsumer<TMessage, THandler>)}.");
            return message;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Failed to deserialize JSON data with value: {jsonData} to type: {typeof(T)}.");
            throw;
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