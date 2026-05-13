using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services.Interfaces;
using Shared.Contracts.Events;

namespace InvoicesWebService.Services.DraftServices;

public sealed class DraftInvoiceService(
    IDraftInvoiceRepository draftRepo,
    IRawTransactionReader txRepo,
    ILogger<DraftInvoiceService> logger)
    : IDraftInvoiceService
{
    public async Task ProcessAggregationReadyAsync(AggregationReadyEvent evt, CancellationToken ct = default)
    {
        var transactions = await txRepo.GetByGroupIdAsync(evt.AggregationGroupId, ct);
        if (transactions.Count == 0)
            throw new InvalidOperationException($"No processed transactions found for group {evt.AggregationGroupId}");

        var draft = await draftRepo.GetByGroupIdAsync(evt.AggregationGroupId, ct);
        
        if (draft is null)
        {
            draft = DraftInvoiceFactory.Create(evt, transactions);
        }
        else
        {
            DraftInvoiceFactory.Update(draft, transactions);
        }
        
        await draftRepo.AddWithLinesAsync(draft, ct);
    }
}