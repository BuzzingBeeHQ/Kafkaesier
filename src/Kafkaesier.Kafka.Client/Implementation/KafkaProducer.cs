using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Kafka.Client.Implementation;

public class KafkaProducer(IOptions<KafkaClientOptions> options) : IDisposable, IKafkaesierProducer
{
    private readonly KafkaClientOptions _options = options.Value;
    private readonly IProducer<string, string> _producer = CreateProducer(options.Value);

    public async Task PublishAsync<TMessage>(KafkaesierCommand<TMessage> command) where TMessage : MessageBase
    {
        var topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
            .WithPrefix(_options.Prefix)
            .Build();

        var kafkaMessage = new Message<string, string>
        {
            Headers = BuildHeaders(command.Metadata),
            Key = command.Key!,
            Value = JsonSerializer.Serialize(command.Message)
        };

        await _producer.ProduceAsync(topicName, kafkaMessage);
    }

    public void Dispose()
    {
        var timeout = TimeSpan.FromMilliseconds(_options.ProducerTimeoutInMilliseconds);
        _producer.Flush(timeout);
        _producer.Dispose();

        GC.SuppressFinalize(this);
    }

    private static IProducer<string, string> CreateProducer(KafkaClientOptions options)
    {
        return new ProducerBuilder<string, string>(options.AsDictionary(KafkaOptionTargets.Producer)).Build();
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