using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.InMemory.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.InMemory.Client.Implementation;

public class InMemoryConsumer<TMessage, THandler>(IServiceProvider serviceProvider, IOptions<InMemoryClientOptions> options) : IKafkaesierConsumer
    where TMessage : MessageBase
    where THandler : CommandHandlerBase<TMessage>
{
    private readonly string _topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
        .WithPrefix(options.Value.Prefix)
        .Build();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InMemoryMessageBroker.CreateChannelOrSkip(_topicName);
        await ExecuteAsync(cancellationToken, _topicName);
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken, string topicName)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var consumeResult = await InMemoryMessageBroker.ConsumeAsync<TMessage>(topicName);
            await HandleMessageInScopeAsync(consumeResult.Message);
        }
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