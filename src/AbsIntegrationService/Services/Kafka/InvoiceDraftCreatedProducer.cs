using AbsIntegrationService.Contracts;
using Messaging.Kafka.Producer;

namespace AbsIntegrationService.Services.Kafka;

public class InvoiceDraftCreatedProducer(IKafkaProducer<InvoiceDraftCreatedEvent>  kafkaProducer) : IInvoiceDraftCreatedProducer
{
    public async Task ProduceDraftCreatedAsync(InvoiceDraftCreatedEvent draftEvent, CancellationToken token)
    {
        await kafkaProducer.ProduceAsync(draftEvent, draftEvent.OperationNumber, token);
    }
}

public interface IInvoiceDraftCreatedProducer
{
    public Task ProduceDraftCreatedAsync(InvoiceDraftCreatedEvent draftEvent, CancellationToken token);
}