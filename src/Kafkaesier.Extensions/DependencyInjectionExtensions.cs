using Kafkaesier.Client;
using Kafkaesier.Client.Abstractions;
using Kafkaesier.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafkaesier.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKafkaesierInfrastructure(this IServiceCollection serviceCollection, Action<KafkaClientOptions>? optionsBuilder = null)
    {
        serviceCollection.AddScoped<IKafkaesierAdminClient, KafkaesierAdminClient>();

        if (optionsBuilder is null)
        {
            return serviceCollection.AddOptions<KafkaClientOptions>().Services;
        }

        return serviceCollection
            .AddOptions<KafkaClientOptions>()
            .Configure(optionsBuilder)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Services;
    }

    public static IServiceCollection AddConsumer<TCommand, THandler>(this IServiceCollection serviceCollection)
        where TCommand : CommandBase
        where THandler : CommandHandlerBase<TCommand>
    {
        return serviceCollection.AddSingleton<IKafkaesierConsumer, KafkaesierConsumer<TCommand, THandler>>();
    }

    public static IServiceCollection AddConsumerWithOptionsOverride<TCommand, THandler>(this IServiceCollection serviceCollection, Action<KafkaClientOptions> optionsBuilder)
        where TCommand : CommandBase
        where THandler : CommandHandlerBase<TCommand>
    {
        return serviceCollection.AddSingleton<IKafkaesierConsumer, KafkaesierConsumer<TCommand, THandler>>(serviceProvider =>
        {
            var kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaClientOptions>>().Value;
            optionsBuilder(kafkaOptions);
            return new KafkaesierConsumer<TCommand, THandler>(serviceProvider, Options.Create(kafkaOptions));
        });
    }

    public static IServiceCollection AddProducer<TKey>(this IServiceCollection serviceCollection) where TKey : class
    {
        return serviceCollection.AddScoped<IKafkaesierProducer<TKey>, KafkaesierProducer<TKey>>();
    }
}