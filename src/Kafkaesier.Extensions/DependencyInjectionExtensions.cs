using Kafkaesier.Client.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kafkaesier.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKafkaesierClient(
        this IServiceCollection serviceCollection,
        Action<KafkaesierClientBuilder> clientBuilder,
        Action<KafkaClientOptions>? optionsBuilder = null)
    {
        var dependencyInjectionContainer = KafkaesierClientBuilder.CreateContainer(serviceCollection, optionsBuilder);
        clientBuilder(dependencyInjectionContainer);
        return serviceCollection;
    }
}