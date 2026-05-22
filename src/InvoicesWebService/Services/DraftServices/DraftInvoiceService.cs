using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services.Interfaces;
using Shared.Contracts.Events;

namespace InvoicesWebService.Services.DraftServices;

public sealed class DraftInvoiceService(
    IDraftInvoiceRepository draftRepo,
    IRawTransactionReader txRepo)
    : IDraftInvoiceService
{
    public async Task ProcessAggregationReadyAsync(AggregationReadyEvent evt, CancellationToken ct = default)
    {
        var transactionTask = txRepo.GetByGroupIdAsync(evt.AggregationGroupId, ct);
        var draftTask = draftRepo.GetByGroupIdAsync(evt.AggregationGroupId, ct);
        
        var transactions = await transactionTask;
        var draft = await draftTask;
        
        if (transactions.Count == 0)
            throw new InvalidOperationException($"No processed transactions found for group {evt.AggregationGroupId}");
        
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