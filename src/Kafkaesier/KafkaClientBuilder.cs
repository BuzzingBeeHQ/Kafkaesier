using Kafkaesier.Abstractions.Commands;
using Kafkaesier.Abstractions.Handlers;
using Kafkaesier.Abstractions.Interfaces;
using Kafkaesier.Kafka.Client.Implementation;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kafkaesier;

public class KafkaClientBuilder
{
    private readonly IServiceCollection _serviceCollection;

    private KafkaClientBuilder(IServiceCollection serviceCollection, Action<KafkaClientOptions>? optionsBuilder = null)
    {
        _serviceCollection = serviceCollection;
        AddKafkaSettings(optionsBuilder);
    }

    private void AddKafkaSettings(Action<KafkaClientOptions>? optionsBuilder = null)
    {
        _serviceCollection.AddKeyedScoped<IKafkaesierAdminClient, KafkaAdminClient>(nameof(KafkaAdminClient));

        if (optionsBuilder is null)
        {
            _serviceCollection.AddOptions<KafkaClientOptions>();
        }
        else
        {
            _serviceCollection.AddOptions<KafkaClientOptions>().Configure(optionsBuilder).ValidateDataAnnotations().ValidateOnStart();
        }
    }

    public KafkaConsumerBuilder<TMessage, THandler> AddConsumer<TMessage, THandler>()
        where TMessage : MessageBase
        where THandler : CommandHandlerBase<TMessage>
    {
        _serviceCollection.AddSingleton<IHostedService, KafkaConsumer<TMessage, THandler>>();
        return KafkaConsumerBuilder<TMessage, THandler>.CreateNew(_serviceCollection, this);
    }

    public KafkaConsumerBuilder<TMessage, THandler> AddConsumerWithOptionsOverride<TMessage, THandler>(Action<KafkaClientOptions> optionsBuilder)
        where TMessage : MessageBase
        where THandler : CommandHandlerBase<TMessage>
    {
        _serviceCollection.AddSingleton<IHostedService, KafkaConsumer<TMessage, THandler>>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaConsumer<TMessage, THandler>(serviceProvider, kafkaOptions);
        });

        return KafkaConsumerBuilder<TMessage, THandler>.CreateNew(_serviceCollection, this);
    }

    public KafkaClientBuilder AddProducer()
    {
        _serviceCollection.AddScoped<IKafkaesierProducer, KafkaProducer>();
        return this;
    }

    public KafkaClientBuilder AddProducerWithOptionsOverride(Action<KafkaClientOptions> optionsBuilder)
    {
        _serviceCollection.AddScoped<IKafkaesierProducer, KafkaProducer>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaProducer(kafkaOptions);
        });

        return this;
    }

    public KafkaClientBuilder AddAdminClient()
    {
        _serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaAdminClient>();
        return this;
    }

    public KafkaClientBuilder AddAdminClientWithOptionsOverride(Action<KafkaClientOptions> optionsBuilder)
    {
        _serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaAdminClient>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaAdminClient(serviceProvider, kafkaOptions);
        });

        return this;
    }

    public static KafkaClientBuilder CreateContainer(IServiceCollection serviceCollection, Action<KafkaClientOptions>? optionsBuilder = null)
    {
        return new KafkaClientBuilder(serviceCollection, optionsBuilder);
    }

    private static IOptions<KafkaClientOptions> OverrideOptions(IServiceProvider serviceProvider, Action<KafkaClientOptions> optionsBuilder)
    {
        var kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaClientOptions>>().Value;
        optionsBuilder(kafkaOptions);
        return Options.Create(kafkaOptions);
    }
}