using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.DraftServices;

public static class DraftInvoiceFactory
{
    public static DraftInvoice Create(AggregationReadyEvent evt, IReadOnlyList<RawTransaction> txs)
    {
        var draft = new DraftInvoice
        {
            Id = Guid.NewGuid(),
            AggregationGroupId = evt.AggregationGroupId,
            TransactionDate = evt.TransactionDate,
            Status = DraftInvoiceStatus.New,
            CurrencyCode = txs.First(t => !string.IsNullOrWhiteSpace(t.CurrencyCode)).CurrencyCode,
            BuyerId= txs.First(t => t.CounterpartyId != null).CounterpartyId!.Value,
            DepartmentId = txs.First(t => t.DepartmentId != null).DepartmentId!.Value,
            Lines = new List<DraftInvoiceLine>()
        };
        
        var grouped = txs.GroupBy(t => new {t.Id, t.ProductCode, t.ProductName, t.UnitMeasure, t.NdsRate });
        
        foreach (var g in grouped)
        {
            var line = new DraftInvoiceLine
            {
                Id = Guid.NewGuid(),
                DraftInvoiceId = draft.Id,
                RawTransactionId = g.Key.Id,
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
        
        draft.TotalNdsAmount = draft.Lines.Sum(l => l.NdsAmount);
        draft.TotalWithNds = draft.Lines.Sum(l => l.TotalAmount);
        draft.TotalWithoutNds = draft.Lines.Sum(l => l.AmountWithoutNds);
        draft.NdsRate = draft.Lines.Any() ? draft.Lines.First().NdsRate : 0m;

        return draft;
    }
}