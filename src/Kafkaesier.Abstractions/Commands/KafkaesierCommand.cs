namespace Kafkaesier.Abstractions.Commands;

public record KafkaesierCommand<TMessage>(TMessage Message, string? Key = null, Dictionary<string, string>? Metadata = null) where TMessage : MessageBase;