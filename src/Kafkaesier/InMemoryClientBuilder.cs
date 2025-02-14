using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.InMemory.Client.Implementation;
using Kafkaesier.InMemory.Client.Options;
using Kafkaesier.Kafka.Client.Implementation;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kafkaesier;

public class InMemoryClientBuilder
{
    private readonly IServiceCollection _serviceCollection;

    private InMemoryClientBuilder(IServiceCollection serviceCollection, Action<InMemoryClientOptions>? optionsBuilder = null)
    {
        _serviceCollection = serviceCollection;
        AddInMemorySettings(optionsBuilder);
    }

    private void AddInMemorySettings(Action<InMemoryClientOptions>? optionsBuilder = null)
    {
        _serviceCollection.AddKeyedScoped<IKafkaesierAdminClient, InMemoryAdminClient>(nameof(InMemoryAdminClient));

        if (optionsBuilder is null)
        {
            _serviceCollection.AddOptions<KafkaClientOptions>();
        }
        else
        {
            _serviceCollection.AddOptions<InMemoryClientOptions>().Configure(optionsBuilder).ValidateDataAnnotations().ValidateOnStart();
        }
    }

    public InMemoryClientBuilder AddConsumer<TMessage, THandler>()
        where TMessage : MessageBase
        where THandler : CommandHandlerBase<TMessage>
    {
        _serviceCollection.AddSingleton<IKafkaesierConsumer, KafkaConsumer<TMessage, THandler>>();
        return this;
    }

    public InMemoryClientBuilder AddProducer()
    {
        _serviceCollection.AddScoped<IKafkaesierProducer, KafkaProducer>();
        return this;
    }

    public InMemoryClientBuilder AddAdminClient()
    {
        _serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaAdminClient>();
        return this;
    }

    public static InMemoryClientBuilder CreateContainer(IServiceCollection serviceCollection, Action<InMemoryClientOptions>? optionsBuilder = null)
    {
        return new InMemoryClientBuilder(serviceCollection, optionsBuilder);
    }
}