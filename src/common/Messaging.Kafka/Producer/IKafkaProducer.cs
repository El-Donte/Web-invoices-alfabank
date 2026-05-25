using Confluent.Kafka;

namespace Messaging.Kafka.Producer;

public interface IKafkaProducer<TMessage> : IDisposable
{
    Task<DeliveryResult<string, TMessage>>? ProduceAsync(TMessage message, string? key = null ,CancellationToken token = default);
}