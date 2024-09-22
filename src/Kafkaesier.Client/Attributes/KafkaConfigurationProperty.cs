using Kafkaesier.Client.Options;

namespace Kafkaesier.Client.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal class KafkaConfigurationProperty(string name, OptionTargets optionTargets) : Attribute
{
    internal string Name { get; set; } = name;
    internal OptionTargets OptionTargets { get; set; } = optionTargets;
}