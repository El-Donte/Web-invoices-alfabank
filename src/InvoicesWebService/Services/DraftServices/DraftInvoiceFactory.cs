using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.DraftServices;

public static class DraftInvoiceFactory
{
    public static DraftInvoice Create(AggregationReadyEvent evt, IReadOnlyList<RawTransaction> txs)
    {
        var validTx = txs.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.CurrencyCode) 
                                              && t is { CounterpartyId: not null, DepartmentId: not null });
        var draft = new DraftInvoice
        {
            Id = Guid.NewGuid(),
            AggregationGroupId = evt.AggregationGroupId,
            TransactionDate = evt.TransactionDate,
            Status = DraftInvoiceStatus.New,
            CurrencyCode = validTx.CurrencyCode,
            BuyerId= validTx.CounterpartyId!.Value,
            DepartmentId = validTx.DepartmentId!.Value,
            Lines = new List<DraftInvoiceLine>()
        };
        
        var grouped = txs.GroupBy(t => 
            new
            {
                t.ProductCode, t.ProductName, t.UnitMeasure, t.NdsRate
            });
        
        foreach (var g in grouped)
        {
            var line = new DraftInvoiceLine
            {
                Id = Guid.NewGuid(),
                DraftInvoiceId = draft.Id,
                RawTransactionId = g.First().Id,
                ProductCode = g.Key.ProductCode,
                ProductName = g.Key.ProductName,
                Unit = g.Key.UnitMeasure,
                NdsRate = g.Key.NdsRate,
                Quantity = g.Sum(t => t.Quantity),
                UnitPrice = g.First().UnitPrice,
                AmountWithoutNds = g.Sum(t => t.Amount - t.NdsAmount),
                NdsAmount = g.Sum(t => t.NdsAmount),
                TotalAmount = g.Sum(t => t.Amount)
            };
            draft.Lines.Add(line);
        }
        
        RecalculateTotals(draft);

        return draft;
    }
    
    public static void Update(DraftInvoice draft, IReadOnlyList<RawTransaction> txs)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (txs == null || txs.Count == 0)
        {
            RecalculateTotals(draft);
            return;
        }
        
        var validTx = txs.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.CurrencyCode) 
                                              && t is { CounterpartyId: not null, DepartmentId: not null });
        if (validTx != null)
        {
            draft.CurrencyCode = validTx.CurrencyCode;
            draft.CurrencyCode = validTx.CurrencyCode;
            draft.BuyerId = validTx.CounterpartyId.Value;
            draft.DepartmentId = validTx.DepartmentId.Value;
        }
        
        draft.Lines.Clear();


        var grouped = txs.GroupBy(t => new { t.ProductCode, t.ProductName, t.UnitMeasure, t.NdsRate });

        foreach (var g in grouped)
        {
            var first = g.First();
            var line = new DraftInvoiceLine
            {
                Id = Guid.NewGuid(),
                DraftInvoiceId = draft.Id,
                RawTransactionId = first.Id,
                ProductCode = g.Key.ProductCode,
                ProductName = g.Key.ProductName,
                Unit = g.Key.UnitMeasure,
                NdsRate = g.Key.NdsRate,
                Quantity = g.Sum(t => t.Quantity),
                UnitPrice = first.UnitPrice,
                AmountWithoutNds = g.Sum(t => t.Amount - t.NdsAmount),
                NdsAmount = g.Sum(t => t.NdsAmount),
                TotalAmount = g.Sum(t => t.Amount)
            };
            draft.Lines.Add(line);
        }

        RecalculateTotals(draft);
    }
        
    private static void RecalculateTotals(DraftInvoice draft)
    {
        draft.TotalNdsAmount = draft.Lines.Sum(l => l.NdsAmount);
        draft.TotalWithNds = draft.Lines.Sum(l => l.TotalAmount);
        draft.TotalWithoutNds = draft.Lines.Sum(l => l.AmountWithoutNds);
        draft.NdsRate = draft.Lines.Count != 0 ? draft.Lines.First().NdsRate : 0m;
    }
}