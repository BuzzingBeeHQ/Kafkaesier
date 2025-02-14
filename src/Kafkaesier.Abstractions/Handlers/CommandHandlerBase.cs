using Kafkaesier.Abstractions.Commands;

namespace Kafkaesier.Abstractions.Handlers;

public abstract class CommandHandlerBase<TMessage> where TMessage : MessageBase
{
    public async Task StartHandleAsync(TMessage message)
    {
        await HandleAsync(message);
    }

    protected abstract Task HandleAsync(TMessage message);
}