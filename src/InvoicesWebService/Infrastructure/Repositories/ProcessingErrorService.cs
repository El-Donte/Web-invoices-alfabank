using InvoicesWebService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class ProcessingErrorService(IDbContextFactory<AppDbContext> ctxFactory) : IProcessingErrorService
{
    public async Task LogAsync(ErrorLogEntry entry, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
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
        catch (DbUpdateException ex)
        {
            throw ex;
        }
    }

    public Task LogBatchAsync(IEnumerable<ErrorLogEntry> errors, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}