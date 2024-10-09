using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Attributes;

namespace Kafkaesier.Client.Options;

public class KafkaClientOptions : KafkaOptionsDictionaryBase
{
    public string Prefix { get; set; } = string.Empty;
    public int ConsumerBlockingTimeoutInMilliseconds { get; set; } = 300;
    public int ProducerTimeoutInMilliseconds { get; set; } = 1000;
    public int AdminClientTimeoutInMilliseconds { get; set; } = 1000;
    public int TopicCreationTimeoutInMilliseconds { get; set; } = 30000;

    [KafkaConfigurationProperty(OptionNames.BootstrapServers, OptionTargets.AdminClient | OptionTargets.Consumer)]
    public string BootstrapServers { get; set; } = "localhost:9092";

    [KafkaConfigurationProperty(OptionNames.AllowAutoCreateTopics, OptionTargets.Consumer)]
    public bool AllowAutoCreateTopics { get; set; } = false;

    [KafkaConfigurationProperty(OptionNames.AutoOffsetReset, OptionTargets.Consumer)]
    public string AutoOffsetReset { get; set; } = "earliest";

    [KafkaConfigurationProperty(OptionNames.EnableAutoCommit, OptionTargets.Consumer)]
    public bool EnableAutoCommit { get; set; } = false;

    [KafkaConfigurationProperty(OptionNames.MaxPartitionFetchBytes, OptionTargets.Consumer)]
    public int MaxPartitionFetchBytes { get; set; } = 10485760;

    [KafkaConfigurationProperty(OptionNames.FetchMaxBytes, OptionTargets.Consumer)]
    public int FetchMaxBytes { get; set; } = 10485760;
}