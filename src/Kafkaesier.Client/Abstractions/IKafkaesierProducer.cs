namespace Kafkaesier.Client.Abstractions;

public interface IKafkaesierProducer<in TKey> where TKey : class
{
    public Task PublishAsync<TCommand>(TCommand command, string? topicName = null, TKey? key = null, Dictionary<string, string>? headers = null);
}