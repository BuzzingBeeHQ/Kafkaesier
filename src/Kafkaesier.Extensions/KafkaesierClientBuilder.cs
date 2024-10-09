using Kafkaesier.Client;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Extensions;

public class KafkaesierClientBuilder
{
    private readonly IServiceCollection _serviceCollection;

    private KafkaesierClientBuilder(IServiceCollection serviceCollection, Action<KafkaClientOptions>? optionsBuilder = null)
    {
        _serviceCollection = serviceCollection;
        AddSettings(optionsBuilder);
    }

    private void AddSettings(Action<KafkaClientOptions>? optionsBuilder = null)
    {
        _serviceCollection.AddKeyedScoped<IKafkaesierAdminClient, KafkaesierAdminClient>(nameof(IKafkaesierConsumer));

        if (optionsBuilder is null)
        {
            _serviceCollection.AddOptions<KafkaClientOptions>();
        }
        else
        {
            _serviceCollection.AddOptions<KafkaClientOptions>().Configure(optionsBuilder).ValidateDataAnnotations().ValidateOnStart();
        }
    }

    public KafkaesierClientBuilder AddConsumer<TCommand, THandler>() where TCommand : CommandBase where THandler : CommandHandlerBase<TCommand>
    {
        _serviceCollection.AddSingleton<IKafkaesierConsumer, KafkaesierConsumer<TCommand, THandler>>();
        return this;
    }

    public KafkaesierClientBuilder AddConsumerWithOptionsOverride<TCommand, THandler>(Action<KafkaClientOptions> optionsBuilder)
        where TCommand : CommandBase
        where THandler : CommandHandlerBase<TCommand>
    {
        _serviceCollection.AddSingleton<IKafkaesierConsumer, KafkaesierConsumer<TCommand, THandler>>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaesierConsumer<TCommand, THandler>(serviceProvider, kafkaOptions);
        });
        return this;
    }

    public KafkaesierClientBuilder AddProducer<TKey>() where TKey : class
    {
        _serviceCollection.AddScoped<IKafkaesierProducer<TKey>, KafkaesierProducer<TKey>>();
        return this;
    }

    public KafkaesierClientBuilder AddProducerWithOptionsOverride<TKey>(Action<KafkaClientOptions> optionsBuilder) where TKey : class
    {
        _serviceCollection.AddScoped<IKafkaesierProducer<TKey>, KafkaesierProducer<TKey>>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaesierProducer<TKey>(kafkaOptions);
        });
        return this;
    }

    public KafkaesierClientBuilder AddAdminClient()
    {
        _serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaesierAdminClient>();
        return this;
    }

    public KafkaesierClientBuilder AddAdminClientWithOptionsOverride(Action<KafkaClientOptions> optionsBuilder)
    {
        _serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaesierAdminClient>(serviceProvider =>
        {
            var kafkaOptions = OverrideOptions(serviceProvider, optionsBuilder);
            return new KafkaesierAdminClient(kafkaOptions);
        });
        return this;
    }

    public static KafkaesierClientBuilder CreateContainer(IServiceCollection serviceCollection, Action<KafkaClientOptions>? optionsBuilder = null)
    {
        return new KafkaesierClientBuilder(serviceCollection, optionsBuilder);
    }

    private static IOptions<KafkaClientOptions> OverrideOptions(IServiceProvider serviceProvider, Action<KafkaClientOptions> optionsBuilder)
    {
        var kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaClientOptions>>().Value;
        optionsBuilder(kafkaOptions);
        return Options.Create(kafkaOptions);
    }
}