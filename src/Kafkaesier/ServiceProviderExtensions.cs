using Kafkaesier.Kafka.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafkaesier;

internal static class ServiceProviderExtensions
{
    internal static ILogger<T> GetLoggerOrDefault<T>(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<T>>();

        if (logger is not null)
        {
            return logger;
        }

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<T>();
    }

    internal static IOptions<KafkaClientOptions> OverrideOptions(this IServiceProvider serviceProvider, Action<KafkaClientOptions> optionsBuilder)
    {
        var defaultOptions = serviceProvider.GetRequiredService<IOptions<KafkaClientOptions>>().Value;
        optionsBuilder(defaultOptions);
        return Options.Create(defaultOptions);
    }
}