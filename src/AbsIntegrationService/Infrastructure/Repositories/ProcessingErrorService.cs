using AbsIntegrationService.Infrastructure.Data;
using Shared;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

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
            RawTransactionId = entry.RawTransactionId
        };
        
        context.ProcessingErrors.Add(error);
        await context.SaveChangesAsync(ct);
    }
}