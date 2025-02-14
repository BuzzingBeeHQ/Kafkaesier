using System.Text;
using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;

namespace Kafkaesier.Abstractions;

public class KafkaesierNameBuilder
{
    private readonly string _baseName;
    private string? _namePrefix;

    private KafkaesierNameBuilder(string baseName)
    {
        _baseName = baseName;
    }

    public KafkaesierNameBuilder WithPrefix(string namePrefix)
    {
        _namePrefix = namePrefix;
        return this;
    }

    public string Build()
    {
        var nameBuilder = new StringBuilder(_baseName);

        if (!string.IsNullOrWhiteSpace(_namePrefix))
        {
            nameBuilder.Insert(0, $"{_namePrefix}.");
        }

        return nameBuilder.ToString();
    }

    public static KafkaesierNameBuilder CreateTopicName<TMessage>() where TMessage : MessageBase
    {
        return new KafkaesierNameBuilder(typeof(TMessage).Name);
    }

    public static KafkaesierNameBuilder CreateConsumerGroupName<THandler, TMessage>()
        where THandler : CommandHandlerBase<TMessage>
        where TMessage : MessageBase
    {
        return new KafkaesierNameBuilder(typeof(THandler).Name);
    }
}