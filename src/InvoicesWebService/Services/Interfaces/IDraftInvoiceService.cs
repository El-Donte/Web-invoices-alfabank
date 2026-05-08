using Shared.Contracts.Events;

namespace InvoicesWebService.Services.Interfaces;

public interface IDraftInvoiceService
{
    Task ProcessAggregationReadyAsync(AggregationReadyEvent evt, CancellationToken ct = default);
}