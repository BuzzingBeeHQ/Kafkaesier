using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Kafka.Client.Attributes;
using Kafkaesier.Kafka.Client.Constants;

namespace Kafkaesier.Kafka.Client.Options;

public class KafkaTopicOptions<TMessage> : KafkaOptionsDictionaryBase where TMessage : MessageBase
{
    public int NumPartitions { get; set; } = 5;
    public short ReplicationFactor { get; set; } = 1;

    [KafkaConfigurationProperty(OptionNames.RetentionMs, KafkaOptionTargets.TopicConfiguration)]
    public int RetentionMs { get; set; } = 60;

    [KafkaConfigurationProperty(OptionNames.MaxMessageBytes, KafkaOptionTargets.TopicConfiguration)]
    public int MaxMessageBytes { get; set; } = 1024 * 1024;
}