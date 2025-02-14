using Kafkaesier.Abstractions.Commands;

namespace Kafkaesier.Abstractions.Interfaces;

public interface IKafkaesierProducer
{
    public Task PublishAsync<TMessage>(KafkaesierCommand<TMessage> command) where TMessage : MessageBase;
}