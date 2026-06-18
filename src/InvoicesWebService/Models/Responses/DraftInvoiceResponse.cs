using Shared.Entities;

namespace InvoicesWebService.Models.Responses;

public record DraftInvoiceResponse(DateTime TransactionDate, decimal NdsRate, decimal TotalNdsAmount, 
    decimal TotalWithNds, decimal TotalWithoutNds, DraftInvoiceStatus Status, DateTime CreatedAt);
