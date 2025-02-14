using Kafkaesier.InMemory.Client.Options;
using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kafkaesier;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKafkaesierKafkaInfrastructure(
        this IServiceCollection serviceCollection,
        Action<KafkaClientBuilder> clientBuilder,
        Action<KafkaClientOptions>? optionsBuilder = null)
    {
        var dependencyInjectionContainer = KafkaClientBuilder.CreateContainer(serviceCollection, optionsBuilder);
        clientBuilder(dependencyInjectionContainer);
        return serviceCollection;
    }

    public static IServiceCollection AddKafkaesierInMemoryInfrastructure(
        this IServiceCollection serviceCollection,
        Action<InMemoryClientBuilder> clientBuilder,
        Action<InMemoryClientOptions>? optionsBuilder = null)
    {
        var dependencyInjectionContainer = InMemoryClientBuilder.CreateContainer(serviceCollection, optionsBuilder);
        clientBuilder(dependencyInjectionContainer);
        return serviceCollection;
    }
}