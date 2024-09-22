using System.Reflection;
using Kafkaesier.Client.Attributes;
using Kafkaesier.Client.Options;

namespace Kafkaesier.Client.Abstractions;

public abstract class KafkaOptionsDictionaryBase
{
    internal Dictionary<string, string?> AsDictionary(OptionTargets optionTarget)
    {
        Dictionary<string, string?> consumerProperties = new();

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            var kafkaConfigurationProperty = property.GetCustomAttribute<KafkaConfigurationPropertyAttribute>();
            if (kafkaConfigurationProperty is not null && kafkaConfigurationProperty.OptionTargets.HasFlag(optionTarget))
            {
                object? optionValue = property.GetValue(this);
                consumerProperties.Add(kafkaConfigurationProperty.Name, optionValue?.ToString());
            }
        }

        return consumerProperties;
    }
}