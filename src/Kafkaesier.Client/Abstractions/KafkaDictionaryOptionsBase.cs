using System.Reflection;
using Kafkaesier.Client.Attributes;
using Kafkaesier.Client.Options;

namespace Kafkaesier.Client.Abstractions;

public abstract class KafkaDictionaryOptionsBase
{
    internal Dictionary<string, string?> AsDictionary(OptionTargets target)
    {
        Dictionary<string, string?> consumerProperties = new();

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            var kafkaConfigurationProperty = property.GetCustomAttribute<KafkaConfigurationProperty>();
            if (kafkaConfigurationProperty is not null && kafkaConfigurationProperty.OptionTargets.HasFlag(target))
            {
                object? optionValue = property.GetValue(this);
                consumerProperties.Add(kafkaConfigurationProperty.Name, optionValue?.ToString());
            }
        }

        return consumerProperties;
    }
}