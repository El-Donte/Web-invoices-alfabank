using Messaging.Kafka.Producer;

namespace AbsMockProducer;

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


// public async Task<IReadOnlyList<ProcessingError>> GetRetryableAsync(ProcessingStage stage, int limit, 
//     CancellationToken ct = default) =>
//     await context.ProcessingErrors
//         .AsNoTracking()
//         .Where(e => e.Stage == stage && e.Retryable && !e.Resolved)
//         .OrderBy(e => e.CreatedAt)
//         .Take(limit)
//         .ToListAsync(ct);
//
// public async Task MarkResolvedAsync(Guid errorId, CancellationToken ct = default) =>
//     await context.ProcessingErrors
//         .Where(e => e.Id == errorId)
//         .ExecuteUpdateAsync(s => s.SetProperty(e => e.Resolved, true), ct);