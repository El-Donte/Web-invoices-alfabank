using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messaging.Kafka.Producer;

public class KafkaProducer<TMessage> : IKafkaProducer<TMessage>
{
    private readonly IProducer<string, TMessage> _producer;
    private readonly ILogger<KafkaProducer<TMessage>> _logger;
    private readonly string _topic;
    private bool _disposed;

    public KafkaProducer(IOptions<KafkaSettings> kafkaSettings,  ILogger<KafkaProducer<TMessage>> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 5,
            RetryBackoffMs = 500,
            EnableIdempotence = true,
            ClientId = $"abs-ingestion-producer-{Guid.NewGuid().ToString("N").Substring(0, 8)}"
        };
        _logger = logger;
        
        _topic = kafkaSettings.Value.Topic;
        
        _producer = new ProducerBuilder<string, TMessage>(config)
            .SetValueSerializer(new KafkaJsonSerializer<TMessage>())
            .Build();
    }

    public async Task ProduceAsync(TMessage message, string? key = null, CancellationToken token = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        var messageKey = key ?? Guid.NewGuid().ToString("N");
        
        var deliveryResult = await _producer.ProduceAsync(_topic, new Message<string, TMessage>
        {
            Key = messageKey,
            Value = message
        }, token);
        
        _logger.LogInformation("Message published to topic {Topic} | Partition: {Partition} | Offset: {Offset} | Key: {Key}",
            deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset, messageKey);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _disposed = true;
    }
}