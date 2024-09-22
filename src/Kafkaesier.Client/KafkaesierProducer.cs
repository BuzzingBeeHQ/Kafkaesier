using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client;

public sealed class KafkaesierProducer<TKey>(IOptions<KafkaClientOptions> options) : IKafkaesierProducer<TKey>, IDisposable where TKey : class
{
    private readonly KafkaClientOptions _kafkaClientOptions = options.Value;
    private readonly IProducer<TKey, string> _kafkaProducer = CreateProducer(options.Value);

    public async Task PublishAsync<TCommand>(TCommand data, string? topicName = null, TKey? key = null, Dictionary<string, string>? headers = null)
    {
        topicName ??= KafkaesierNameBuilder.CreateTopicName<TCommand>()
            .WithPrefix(_kafkaClientOptions.NamePrefix)
            .Build();

        var payload = data is string ? data.ToString() : JsonSerializer.Serialize(data);
        var kafkaMessage = new Message<TKey, string>
        {
            Headers = BuildHeaders(headers),
            Key = key,
            Value = payload
        };

        await _kafkaProducer.ProduceAsync(topicName, kafkaMessage);
    }

    public void Dispose()
    {
        var timeout = TimeSpan.FromMilliseconds(_kafkaClientOptions.ProducerTimeoutInMilliseconds);
        _kafkaProducer.Flush(timeout);
        _kafkaProducer.Dispose();
    }

    private static IProducer<TKey, string> CreateProducer(KafkaClientOptions options)
    {
        return new ProducerBuilder<TKey, string>(options.AsDictionary(OptionTargets.Producer)).Build();
    }

    private static Headers BuildHeaders(Dictionary<string, string>? headers = null)
    {
        var kafkaHeaders = new Headers();

        if (headers is not null)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                kafkaHeaders.Add(new Header(header.Key, Encoding.UTF8.GetBytes(header.Value)));
            }
        }

        return kafkaHeaders;
    }
}