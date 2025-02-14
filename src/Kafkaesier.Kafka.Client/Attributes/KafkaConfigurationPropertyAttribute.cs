using Kafkaesier.Kafka.Client.Options;

namespace Kafkaesier.Kafka.Client.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal class KafkaConfigurationPropertyAttribute(string name, KafkaOptionTargets optionTargets) : Attribute
{
    internal string Name { get; set; } = name;
    internal KafkaOptionTargets OptionTargets { get; set; } = optionTargets;
}