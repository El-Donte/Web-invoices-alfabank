using Messaging.Kafka.Consumer;
using Messaging.Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Kafka;

public static class Extensions
{
    public static void AddProducer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        var settings = configurationSection.Get<KafkaSettings>();
        services.AddSingleton<IKafkaProducer<TMessage>>(sp =>
             ActivatorUtilities.CreateInstance<KafkaProducer<TMessage>>(sp, settings));
    }

    public static IServiceCollection AddConsumer<TMessage, THandler>(this IServiceCollection services,
        IConfigurationSection configurationSection) where THandler : class, IMessageHandler<TMessage>
    {
        var settings = configurationSection.Get<KafkaSettings>();
        
        services.AddHostedService<KafkaConsumer<TMessage>>(sp =>
            ActivatorUtilities.CreateInstance<KafkaConsumer<TMessage>>(sp, settings));
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        
        return services;
    }
}