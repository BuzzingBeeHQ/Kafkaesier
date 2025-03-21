using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.InMemory.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafkaesier.InMemory.Client.Implementation;

public class InMemoryConsumer<TMessage, THandler>(
    IServiceProvider serviceProvider,
    ILogger<IKafkaesierConsumer> logger,
    IOptions<InMemoryClientOptions> options)
    : IKafkaesierConsumer
    where TMessage : MessageBase
    where THandler : CommandHandlerBase<TMessage>
{
    private readonly string _topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
        .WithPrefix(options.Value.Prefix)
        .Build();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        InMemoryMessageBroker.CreateChannelOrSkip(_topicName);

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
                    logger.LogError(exception, "Unhandled error in InMemory consumer: {ExceptionType}. Details: {Message}", exception.GetType().Name, exception.Message);
                }
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private async Task ExecuteAsync()
    {
        var consumeResult = await InMemoryMessageBroker.ConsumeAsync<TMessage>(_topicName);
        await HandleMessageInScopeAsync(consumeResult.Message);
    }

    private async Task HandleMessageInScopeAsync(TMessage message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handlerInstance = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider);
        await handlerInstance.StartHandleAsync(message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        InMemoryMessageBroker.CloseChannel(_topicName);
        return Task.CompletedTask;
    }
}