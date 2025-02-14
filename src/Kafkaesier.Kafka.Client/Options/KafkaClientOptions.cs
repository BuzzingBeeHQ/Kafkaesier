using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Attributes;
using Kafkaesier.Kafka.Client.Constants;

namespace Kafkaesier.Kafka.Client.Options;

public class KafkaClientOptions : KafkaOptionsDictionaryBase, IKafkaesierClientOptions
{
    public string Prefix { get; set; } = string.Empty;
    public int ConsumerBlockingTimeoutInMilliseconds { get; set; } = 300;
    public int ProducerTimeoutInMilliseconds { get; set; } = 1000;
    public int AdminClientTimeoutInMilliseconds { get; set; } = 1000;
    public int TopicCreationTimeoutInMilliseconds { get; set; } = 30000;

    [KafkaConfigurationProperty(OptionNames.BootstrapServers, KafkaOptionTargets.AdminClient | KafkaOptionTargets.Consumer)]
    public string BootstrapServers { get; set; } = "localhost:9092";

    [KafkaConfigurationProperty(OptionNames.AllowAutoCreateTopics, KafkaOptionTargets.Consumer)]
    public bool AllowAutoCreateTopics { get; set; } = false;

    [KafkaConfigurationProperty(OptionNames.AutoOffsetReset, KafkaOptionTargets.Consumer)]
    public string AutoOffsetReset { get; set; } = "earliest";

    [KafkaConfigurationProperty(OptionNames.EnableAutoCommit, KafkaOptionTargets.Consumer)]
    public bool EnableAutoCommit { get; set; } = false;

    [KafkaConfigurationProperty(OptionNames.MaxPartitionFetchBytes, KafkaOptionTargets.Consumer)]
    public int MaxPartitionFetchBytes { get; set; } = 10485760;

    [KafkaConfigurationProperty(OptionNames.FetchMaxBytes, KafkaOptionTargets.Consumer)]
    public int FetchMaxBytes { get; set; } = 10485760;
}