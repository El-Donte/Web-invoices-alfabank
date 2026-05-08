using InvoicesWebService.Infrastructure.Data;
using Shared;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class ProcessingErrorService(AppDbContext context) : IProcessingErrorService
{
    public async Task LogAsync(ErrorLogEntry entry, CancellationToken ct = default)
    {
        var error = new ProcessingError
        {
            Stage = entry.Stage,
            Code = entry.Code,
            Message = entry.Message,
            Payload = entry.Payload,
            Retryable = entry.Retryable,
            AggregationGroupId = entry.AggregationGroupId,
            RawTransactionId = entry.RawTransactionId,
            DraftInvoiceId = entry.DraftInvoiceId,
            InvoiceId = entry.InvoiceId,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.ProcessingErrors.Add(error);
        await context.SaveChangesAsync(ct);
    }
}