namespace Messaging.Kafka.Producer;

public interface IKafkaProducer<in TMessage> : IDisposable
{
    Task ProduceAsync(TMessage message, string? key = null ,CancellationToken token = default);
}