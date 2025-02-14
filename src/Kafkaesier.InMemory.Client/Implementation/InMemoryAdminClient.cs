using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.InMemory.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.InMemory.Client.Implementation;

public class InMemoryAdminClient(IOptions<InMemoryClientOptions> options) : IKafkaesierAdminClient
{
    private readonly InMemoryClientOptions _options = options.Value;

    public Task<string> CreateTopicOrSkipAsync<TMessage>() where TMessage : MessageBase
    {
        var topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
            .WithPrefix(_options.Prefix)
            .Build();

        InMemoryMessageBroker.CreateChannelOrSkip(topicName);
        return Task.FromResult(topicName);
    }
}