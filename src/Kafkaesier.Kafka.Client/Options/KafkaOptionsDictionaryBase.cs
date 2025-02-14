using System.Reflection;
using Kafkaesier.Kafka.Client.Attributes;

namespace Kafkaesier.Kafka.Client.Options;

public abstract class KafkaOptionsDictionaryBase
{
    public Dictionary<string, string?> AsDictionary(KafkaOptionTargets optionTargets)
    {
        Dictionary<string, string?> consumerProperties = new();

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            var kafkaConfigurationProperty = property.GetCustomAttribute<KafkaConfigurationPropertyAttribute>();
            if (kafkaConfigurationProperty is not null && kafkaConfigurationProperty.OptionTargets.HasFlag(optionTargets))
            {
                object? optionValue = property.GetValue(this);
                consumerProperties.Add(kafkaConfigurationProperty.Name, optionValue?.ToString());
            }
        }

        return consumerProperties;
    }
}