using InvoicesWebService.Models.DTOs;
using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.DraftServices;

public static class DraftInvoiceFactory
{
    public static DraftInvoice Create(AggregationReadyEvent evt, IReadOnlyList<RawTransactionDTO> txs)
    {
        var validTx = ValidTransaction(txs);
        var draft = new DraftInvoice
        {
            Id = Guid.NewGuid(),
            AggregationGroupId = evt.AggregationGroupId,
            TransactionDate = evt.TransactionDate,
            Status = DraftInvoiceStatus.New,
            CurrencyCode = validTx.CurrencyCode,
            BuyerId= validTx.CounterpartyId,
            DepartmentId = validTx.DepartmentId,
            Lines = new List<DraftInvoiceLine>()
        };
        
        AddLines(draft, txs);
        
        RecalculateTotals(draft);

        return draft;
    }
    
    public static void Update(DraftInvoice draft, IReadOnlyList<RawTransactionDTO> txs)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (txs == null || txs.Count == 0)
        {
            RecalculateTotals(draft);
            return;
        }
        
        var validTx = ValidTransaction(txs);
        if (validTx != null)
        {
            draft.CurrencyCode = validTx.CurrencyCode;
            draft.CurrencyCode = validTx.CurrencyCode;
            draft.BuyerId = validTx.CounterpartyId;
            draft.DepartmentId = validTx.DepartmentId;
        }
        
        draft.Lines.Clear();
        
        AddLines(draft, txs);

        RecalculateTotals(draft);
    }
        
    private static void RecalculateTotals(DraftInvoice draft)
    {
        draft.TotalNdsAmount = draft.Lines.Sum(l => l.NdsAmount);
        draft.TotalWithNds = draft.Lines.Sum(l => l.TotalAmount);
        draft.TotalWithoutNds = draft.Lines.Sum(l => l.AmountWithoutNds);
        draft.NdsRate = draft.Lines.Count != 0 ? draft.Lines.First().NdsRate : 0m;
    }

    private static void AddLines(DraftInvoice draft, IReadOnlyList<RawTransactionDTO> txs)
    {
        var grouped = txs.GroupBy(t => 
            new
            {
                t.ProductCode, t.ProductName, t.UnitMeasure, t.NdsRate
            });
        
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
    }
    
    private static RawTransactionDTO? ValidTransaction(IReadOnlyList<RawTransactionDTO> txs) =>
        txs.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.CurrencyCode) 
                                && t.CounterpartyId != Guid.Empty && t.DepartmentId != Guid.Empty);
}