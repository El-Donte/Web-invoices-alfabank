using Confluent.Kafka;

namespace Messaging.Kafka.Producer;

public class KafkaProducer<TMessage> : IKafkaProducer<TMessage>
{
    private readonly IProducer<string, TMessage> _producer;
    private readonly string _topic;
    private bool _disposed;

    public KafkaProducer(KafkaSettings kafkaSettings)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 5,
            BatchNumMessages = 50_000,
            RetryBackoffMs = 500,
            EnableIdempotence = true,
            LingerMs = 20,
            BatchSize = 16_777_216,
            QueueBufferingMaxMessages = 5_000_000,
            ClientId = $"invoice-system-producer-{Guid.NewGuid().ToString("N").Substring(0, 8)}"
        };
        
        _topic = kafkaSettings.Topic;
        
        _producer = new ProducerBuilder<string, TMessage>(config)
            .SetValueSerializer(new KafkaJsonSerializer<TMessage>())
            .Build();
    }

    public async Task<DeliveryResult<string, TMessage>>? ProduceAsync(TMessage message, string? key = null, CancellationToken token = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        var messageKey = key ?? Guid.NewGuid().ToString("N");
        
        var deliveryResult = await _producer.ProduceAsync(_topic, new Message<string, TMessage>
        {
            Key = messageKey,
            Value = message
        }, token);

        return deliveryResult;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _disposed = true;
    }
}