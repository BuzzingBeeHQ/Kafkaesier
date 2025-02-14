namespace Kafkaesier.Kafka.Client.Constants;

internal static class OptionNames
{
    internal const string GroupId = "group.id";
    internal const string BootstrapServers = "bootstrap.servers";
    internal const string AllowAutoCreateTopics = "allow.auto.create.topics";
    internal const string AutoOffsetReset = "auto.offset.reset";
    internal const string EnableAutoCommit = "enable.auto.commit";
    internal const string MaxPartitionFetchBytes = "max.partition.fetch.bytes";
    internal const string FetchMaxBytes = "fetch.max.bytes";
    internal const string RetentionMs = "retention.ms";
    internal const string MaxMessageBytes = "max.message.bytes";
}