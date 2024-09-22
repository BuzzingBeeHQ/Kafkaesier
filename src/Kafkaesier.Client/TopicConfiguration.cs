using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Attributes;
using Kafkaesier.Client.Options;

namespace Kafkaesier.Client;

public class TopicConfiguration : KafkaDictionaryOptionsBase
{
    public int NumPartitions { get; set; } = 5;
    public short ReplicationFactor { get; set; } = 1;

    [KafkaConfigurationProperty(OptionNames.RetentionMs, OptionTargets.TopicConfiguration)]
    public int RetentionMs { get; set; } = 60;

    [KafkaConfigurationProperty(OptionNames.MaxMessageBytes, OptionTargets.TopicConfiguration)]
    public int MaxMessageBytes { get; set; } = 1024 * 1024;
}