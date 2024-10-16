using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client;

public class KafkaesierProducer<TKey>(IOptions<KafkaClientOptions> options) : IKafkaesierProducer<TKey>, IDisposable where TKey : class
{
    private readonly KafkaClientOptions _kafkaClientOptions = options.Value;
    private readonly IProducer<TKey, string> _kafkaProducer = CreateProducer(options.Value);

    public async Task PublishAsync<TCommand>(TCommand command, string? topicName = null, TKey? key = null, Dictionary<string, string>? headers = null)
    {
        topicName ??= KafkaesierNameBuilder.CreateTopicName<TCommand>()
            .WithPrefix(_kafkaClientOptions.Prefix)
            .Build();

        string messagePayload = command switch
        {
            _ when command is string serialized => serialized,
            _ => JsonSerializer.Serialize(command)
        };
        var kafkaMessage = new Message<TKey, string>
        {
            Headers = BuildHeaders(headers),
            Key = key!,
            Value = messagePayload
        };

        await _kafkaProducer.ProduceAsync(topicName, kafkaMessage);
    }

    public void Dispose()
    {
        var timeout = TimeSpan.FromMilliseconds(_kafkaClientOptions.ProducerTimeoutInMilliseconds);
        _kafkaProducer.Flush(timeout);
        _kafkaProducer.Dispose();

        GC.SuppressFinalize(this);
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
                kafkaHeaders.Add(new Header(header.Key, Encoding.Unicode.GetBytes(header.Value)));
            }
        }

        return kafkaHeaders;
    }
}