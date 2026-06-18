using Confluent.Kafka;
using Shared.Contracts.Events;

namespace InvoicesWebService.Services.Interfaces;

public interface IDraftInvoiceCreationService
{
    Task ProcessAggregationReadyAsync(List<ConsumeResult<string, AggregationReadyEvent>> batch, CancellationToken ct = default);
}