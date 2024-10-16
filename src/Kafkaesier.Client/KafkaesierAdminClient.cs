using System.Diagnostics;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Client;

public sealed class KafkaesierAdminClient(IOptions<KafkaClientOptions> options) : IKafkaesierAdminClient, IDisposable
{
    private readonly KafkaClientOptions _kafkaClientOptions = options.Value;
    private readonly IAdminClient _kafkaAdminClient = CreateAdminClient(options.Value);

    public async Task<string> CreateTopicOrSkipAsync<TCommand>() where TCommand : CommandBase
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Exception? lastException;
        do
        {
            try
            {
                return await CreateTopicAsync<TCommand>();
            }
            catch (Exception exception)
            {
                lastException = exception;
                await Task.Delay(_kafkaClientOptions.AdminClientTimeoutInMilliseconds);
            }
        }
        while (stopwatch.ElapsedMilliseconds <= _kafkaClientOptions.TopicCreationTimeoutInMilliseconds);
        throw lastException;
    }

    public void Dispose()
    {
        _kafkaAdminClient.Dispose();
    }

    private async Task<string> CreateTopicAsync<TCommand>() where TCommand : CommandBase
    {
        var topicName = KafkaesierNameBuilder.CreateTopicName<TCommand>()
            .WithPrefix(_kafkaClientOptions.Prefix)
            .Build();
        var commandInstance = Activator.CreateInstance<TCommand>();

        var topicSpecification = BuildTopicSpecification(topicName, commandInstance.GetConfiguration());
        if (IsTopicExists(topicSpecification))
        {
            return topicName;
        }

        await _kafkaAdminClient.CreateTopicsAsync([ topicSpecification ]);
        return topicName;
    }

    private bool IsTopicExists(TopicSpecification topicSpecification)
    {
        var timeout = TimeSpan.FromMilliseconds(_kafkaClientOptions.AdminClientTimeoutInMilliseconds);
        var metadata = _kafkaAdminClient.GetMetadata(timeout);
        return metadata.Topics.Any(meta => meta.Topic == topicSpecification.Name);
    }

    private static IAdminClient CreateAdminClient(KafkaClientOptions options)
    {
        return new AdminClientBuilder(options.AsDictionary(OptionTargets.AdminClient)).Build();
    }

    private static TopicSpecification BuildTopicSpecification(string topicName, TopicConfiguration topicConfiguration)
    {
        return new TopicSpecification
        {
            Name = topicName,
            NumPartitions = topicConfiguration.NumPartitions,
            ReplicationFactor = topicConfiguration.ReplicationFactor != 1 ? topicConfiguration.ReplicationFactor : (short)-1,
            Configs = topicConfiguration.AsDictionary(OptionTargets.TopicConfiguration)
        };
    }
}