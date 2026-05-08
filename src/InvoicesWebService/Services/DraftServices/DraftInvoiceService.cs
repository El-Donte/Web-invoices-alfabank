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

        if (await draftRepo.ExistsByGroupIdAsync(evt.AggregationGroupId, ct))
        {
            logger.LogDebug("Draft already exists for group {GroupId}. Idempotent skip.", evt.AggregationGroupId);
            return;
        }
        
        var transactions = await txRepo.GetByGroupIdAsync(evt.AggregationGroupId, ct);
        if (transactions.Count == 0)
            throw new InvalidOperationException($"No processed transactions found for group {evt.AggregationGroupId}");
        
        var draft = DraftInvoiceFactory.Create(evt, transactions);
        
        await draftRepo.AddWithLinesAsync(draft, ct);
        logger.LogInformation("DraftInvoice {DraftId} created for group {GroupId}.", draft.Id, evt.AggregationGroupId);
    }
}