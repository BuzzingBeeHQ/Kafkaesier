using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Kafkaesier.Abstractions.Commands;

namespace Kafkaesier.InMemory.Client;

internal class InMemoryMessageBroker
{
    private readonly ConcurrentDictionary<string, Channel<string>> _channels = new();

    private static InMemoryMessageBroker Instance { get; } = new();

    private InMemoryMessageBroker() { }

    internal static void CreateChannelOrSkip(string topicName)
    {
        if (!Instance.HasChannel(topicName))
        {
            Instance.SetNewChannel(topicName);
        }
    }

    internal static void CloseChannel(string topicName)
    {
        if (Instance.TryGetChannel(topicName, out Channel<string> channel))
        {
            channel.Writer.Complete();
            Instance.RemoveChannel(topicName);
        }
    }

    internal static async Task ProduceAsync<TMessage>(string topicName, KafkaesierCommand<TMessage> command) where TMessage : MessageBase
    {
        if (Instance.TryGetChannel(topicName, out Channel<string> channel))
        {
            var serializedCommand = JsonSerializer.Serialize(command);
            await channel.Writer.WriteAsync(serializedCommand);
        }
    }

    internal static async Task<KafkaesierCommand<TMessage>> ConsumeAsync<TMessage>(string topicName) where TMessage : MessageBase
    {
        if (Instance.TryGetChannel(topicName, out Channel<string> channel))
        {
            var command = await channel.Reader.ReadAsync();
            return JsonSerializer.Deserialize<KafkaesierCommand<TMessage>>(command)!;
        }

        throw new Exception("Channel does not exist.");
    }

    private bool HasChannel(string topicName)
    {
        return _channels.ContainsKey(topicName);
    }

    private bool TryGetChannel(string topicName, out Channel<string> existingChannel)
    {
        return _channels.TryGetValue(topicName, out existingChannel);
    }

    private void SetNewChannel(string topicName)
    {
        var channel = Channel.CreateUnbounded<string>();
        _channels[topicName] = channel;
    }

    private void RemoveChannel(string topicName)
    {
        _channels.Remove(topicName, out _);
    }
}