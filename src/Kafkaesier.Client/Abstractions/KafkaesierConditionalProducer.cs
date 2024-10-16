using Kafkaesier.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client.Abstractions;

public abstract class KafkaesierConditionalProducer<TKey>(IOptions<KafkaClientOptions> options) : KafkaesierProducer<TKey>(options) where TKey : class
{
    protected abstract Task<bool> CanProduceAsync<TCommand>(TCommand command);

    public new async Task PublishAsync<TCommand>(TCommand command, string? topicName = null, TKey? key = null, Dictionary<string, string>? headers = null)
    {
        if (await CanProduceAsync(command))
        {
            await base.PublishAsync(command, topicName, key, headers);
        }
    }
}