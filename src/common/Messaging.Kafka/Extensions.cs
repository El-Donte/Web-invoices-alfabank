using Messaging.Kafka.Consumer;
using Messaging.Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Kafka;

public static class Extensions
{
    public static void AddProducer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.Configure<KafkaSettings>(configurationSection);
        services.AddSingleton<IKafkaProducer<TMessage>, KafkaProducer<TMessage>>();
    }

    public static IServiceCollection AddConsumer<TMessage, THandler>(this IServiceCollection services,
        IConfigurationSection configurationSection) where THandler : class, IMessageHandler<TMessage>
    {
        services.Configure<KafkaSettings>(configurationSection);
        services.AddHostedService<KafkaConsumer<TMessage>>();
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        
        return services;
    }
}