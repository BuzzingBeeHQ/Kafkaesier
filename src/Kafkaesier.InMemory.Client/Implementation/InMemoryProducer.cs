using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.InMemory.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.InMemory.Client.Implementation;

public class InMemoryProducer(IOptions<InMemoryClientOptions> options) : IKafkaesierProducer
{
    public async Task PublishAsync<TMessage>(KafkaesierCommand<TMessage> command) where TMessage : MessageBase
    {
        var topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
            .WithPrefix(options.Value.Prefix)
            .Build();

        await InMemoryMessageBroker.ProduceAsync(topicName, command);
    }
}