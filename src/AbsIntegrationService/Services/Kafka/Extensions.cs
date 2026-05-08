using Shared.Contracts;

namespace AbsIntegrationService.Services.Kafka;

public static class Extensions
{
    public static IServiceCollection AddConsumer(this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        var settings = configurationSection.Get<KafkaSettings>();
        
        services.AddHostedService<RawTransactionConsumer>(sp =>
            ActivatorUtilities.CreateInstance<RawTransactionConsumer>(sp, settings,new JsonDeserializer<AbsMessage>()));
        
        return services;
    }
}