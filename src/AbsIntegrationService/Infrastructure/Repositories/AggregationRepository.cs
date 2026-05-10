using AbsIntegrationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class AggregationRepository(AppDbContext context) : IAggregationRepository
{
    public async Task<AggregationGroup?> GetByOperationNumberAsync(string operationNumber, CancellationToken ct = default) =>
        await context.AggregationGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.OperationNumber == operationNumber, ct);

    public async Task CreateOrUpdateAsync(AggregationGroup group, CancellationToken ct = default)
    {
        var existing = await context.AggregationGroups.FindAsync([group.Id], ct);

        if (existing is null)
        {
            context.AggregationGroups.Add(group);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(group);
        }
        
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AggregationGroup>> GetReadyForDraftAsync(int limit, CancellationToken ct = default) =>
        await context.AggregationGroups
            .AsNoTracking()
            .Where(g => g.Status == AggregationStatus.Ready)
            .OrderBy(g => g.ReadyAt)
            .Take(limit)
            .ToListAsync(ct);
}