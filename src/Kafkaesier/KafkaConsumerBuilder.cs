using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kafkaesier;

public class KafkaConsumerBuilder<TMessage, THandler>
    where TMessage : MessageBase
    where THandler : class
{
    private readonly IServiceCollection _serviceCollection;
    private readonly KafkaClientBuilder _clientBuilder;

    private KafkaConsumerBuilder(IServiceCollection serviceCollection, KafkaClientBuilder clientBuilder)
    {
        _serviceCollection = serviceCollection;
        _clientBuilder = clientBuilder;
    }

    public KafkaClientBuilder ConfigureTopic(Action<KafkaTopicOptions<TMessage>> configure)
    {
        var topicOptions = new KafkaTopicOptions<TMessage>();
        configure(topicOptions);

        AddOptionsToServiceCollection(topicOptions);

        return _clientBuilder;
    }

    private void AddOptionsToServiceCollection(KafkaTopicOptions<TMessage>? topicOptions = null)
    {
        topicOptions ??= new KafkaTopicOptions<TMessage>();
        _serviceCollection.RemoveAll(typeof(KafkaTopicOptions<TMessage>));
        _serviceCollection.AddSingleton(topicOptions);
    }

    public static KafkaConsumerBuilder<TMessage, THandler> CreateNew(IServiceCollection serviceCollection, KafkaClientBuilder clientBuilder)
    {
        return new KafkaConsumerBuilder<TMessage, THandler>(serviceCollection, clientBuilder);
    }
}