namespace Kafkaesier.Client.Abstractions;

public interface IKafkaesierAdminClient
{
    public Task<string> CreateTopicOrSkipAsync<TCommand>() where TCommand : CommandBase;
}