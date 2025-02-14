using Kafkaesier.Abstractions.Commands;

namespace Kafkaesier.Abstractions.Interfaces;

public interface IKafkaesierAdminClient
{
    public Task<string> CreateTopicOrSkipAsync<TMessage>() where TMessage : MessageBase;
}