using Messaging.Kafka.Producer;

namespace InvoicesWebService.Services.Kafka;

public class InvoiceTestCreateProducer(IKafkaProducer<InvoiceTestCreatedMessage>  kafkaProducer) : IInvoiceTestCreatedProducer
{
    public async Task ProduceDraftCreatedAsync(InvoiceTestCreatedMessage testMessage, CancellationToken token)
    {
        await kafkaProducer.ProduceAsync(testMessage, testMessage.OperationNumber, token);
    }
}

public interface IInvoiceTestCreatedProducer
{
    public Task ProduceDraftCreatedAsync(InvoiceTestCreatedMessage testMessage, CancellationToken token);
}