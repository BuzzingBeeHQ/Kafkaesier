namespace Kafkaesier.Client.Abstractions;

public abstract class CommandHandlerBase<TCommand> where TCommand : CommandBase
{
    public async Task StartHandleAsync(TCommand command)
    {
        await HandleAsync(command);
    }

    protected abstract Task HandleAsync(TCommand command);
}