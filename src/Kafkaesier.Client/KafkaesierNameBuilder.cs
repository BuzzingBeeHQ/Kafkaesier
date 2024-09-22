using System.Text;

namespace Kafkaesier.Client;

internal class KafkaesierNameBuilder
{
    private readonly string _baseName;
    private string? _namePrefix;
    private string? _messagePriority;

    private KafkaesierNameBuilder(string baseName)
    {
        _baseName = baseName;
    }

    internal KafkaesierNameBuilder WithPrefix(string namePrefix)
    {
        _namePrefix = namePrefix;
        return this;
    }

    internal KafkaesierNameBuilder WithPriority(MessagePriority messagePriority)
    {
        _messagePriority = Enum.GetName(messagePriority);
        return this;
    }

    internal string Build()
    {
        var nameBuilder = new StringBuilder(_baseName);

        if (_namePrefix is not null)
        {
            nameBuilder.Insert(0, $"{_namePrefix}.");
        }
        if (_messagePriority is not null)
        {
            nameBuilder.Append($".{_messagePriority}");
        }

        return nameBuilder.ToString();
    }

    internal static KafkaesierNameBuilder CreateTopicName<TCommand>()
    {
        return new KafkaesierNameBuilder(typeof(TCommand).Name);
    }

    internal static KafkaesierNameBuilder CreateConsumerGroupName<THandler>()
    {
        return new KafkaesierNameBuilder(typeof(THandler).Name);
    }
}