using System.Diagnostics;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Kafkaesier.Abstractions;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Kafka.Client.Implementation;

public sealed class KafkaAdminClient(IServiceProvider serviceProvider, IOptions<KafkaClientOptions> options) : IKafkaesierAdminClient, IDisposable
{
    private readonly KafkaClientOptions _options = options.Value;
    private readonly IAdminClient _adminClient = CreateAdminClient(options.Value);

    public async Task<string> CreateTopicOrSkipAsync<TMessage>() where TMessage : MessageBase
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception? lastException;
        do
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var topicOptions = scope.ServiceProvider.GetRequiredService<KafkaTopicOptions<TMessage>>();
                return await CreateTopicAsync(topicOptions);
            }
            catch (Exception exception)
            {
                lastException = exception;
                await Task.Delay(_options.AdminClientTimeoutInMilliseconds);
            }
        }
        while (stopwatch.ElapsedMilliseconds <= _options.TopicCreationTimeoutInMilliseconds);
        throw lastException;
    }

    public void Dispose()
    {
        _adminClient.Dispose();
    }

    private async Task<string> CreateTopicAsync<TMessage>(KafkaTopicOptions<TMessage> topicOptions) where TMessage : MessageBase
    {
        var topicName = KafkaesierNameBuilder.CreateTopicName<TMessage>()
            .WithPrefix(_options.Prefix)
            .Build();

        var topicSpecification = BuildTopicSpecification(topicName, topicOptions);
        if (IsTopicExists(topicSpecification))
        {
            return topicName;
        }

        await _adminClient.CreateTopicsAsync([topicSpecification]);
        return topicName;
    }

    private bool IsTopicExists(TopicSpecification topicSpecification)
    {
        var timeout = TimeSpan.FromMilliseconds(_options.AdminClientTimeoutInMilliseconds);
        var metadata = _adminClient.GetMetadata(timeout);
        return metadata.Topics.Any(meta => meta.Topic == topicSpecification.Name);
    }

    private static IAdminClient CreateAdminClient(KafkaClientOptions options)
    {
        return new AdminClientBuilder(options.AsDictionary(KafkaOptionTargets.AdminClient)).Build();
    }

    private static TopicSpecification BuildTopicSpecification<TMessage>(string topicName, KafkaTopicOptions<TMessage> topicOptions) where TMessage : MessageBase
    {
        return new TopicSpecification
        {
            Name = topicName,
            NumPartitions = topicOptions.NumPartitions,
            ReplicationFactor = topicOptions.ReplicationFactor != 1 ? topicOptions.ReplicationFactor : (short)-1,
            Configs = topicOptions.AsDictionary(KafkaOptionTargets.TopicConfiguration)
        };
    }
}