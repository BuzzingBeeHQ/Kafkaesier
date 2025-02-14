using Kafkaesier.Abstractions.Interfaces;

namespace Kafkaesier.InMemory.Client.Options;

public class InMemoryClientOptions : IKafkaesierClientOptions
{
    public string Prefix { get; set; } = string.Empty;
}