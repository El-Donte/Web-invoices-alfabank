using Messaging.Kafka.Consumer;
using Messaging.Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messaging.Kafka;

public static class Extensions
{
    public static void AddProducer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        var settings = configurationSection.Get<KafkaSettings>();
        services.AddSingleton<IKafkaProducer<TMessage>>(sp =>
             ActivatorUtilities.CreateInstance<KafkaProducer<TMessage>>(sp, settings));
    }

    public static void AddConsumer<TMessage, TKafkaConsumer>(this IServiceCollection services,
        IConfigurationSection configurationSection) where TKafkaConsumer : BackgroundService
    {
        var settings = configurationSection.Get<KafkaSettings>();
        
        services.AddHostedService<TKafkaConsumer>(sp =>
            ActivatorUtilities.CreateInstance<TKafkaConsumer>(sp, settings, new KafkaJsonDeserializer<TMessage>()));
    }
}