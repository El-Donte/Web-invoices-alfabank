using AbsIntegrationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class ProcessingErrorRepository(AppDbContext context) : IProcessingErrorRepository
{
    public async Task LogAsync(ProcessingError error, CancellationToken ct = default)
    { 
        context.ProcessingErrors.Add(error);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProcessingError>> GetRetryableAsync(ProcessingStage stage, int limit, 
        CancellationToken ct = default) =>
        await context.ProcessingErrors
            .AsNoTracking()
            .Where(e => e.Stage == stage && e.Retryable && !e.Resolved)
            .OrderBy(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task MarkResolvedAsync(Guid errorId, CancellationToken ct = default) =>
        await context.ProcessingErrors
            .Where(e => e.Id == errorId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Resolved, true), ct);
}