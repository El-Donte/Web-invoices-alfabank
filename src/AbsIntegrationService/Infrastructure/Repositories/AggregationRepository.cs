using AbsIntegrationService.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class AggregationRepository(IDbContextFactory<AppDbContext> ctxFactory) : IAggregationRepository
{
    public async Task<AggregationGroup?> GetByOperationNumberAsync(string operationNumber, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        
        return await context.AggregationGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.OperationNumber == operationNumber, ct);
    }

    public async Task CreateOrUpdateAsync(AggregationGroup group, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
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

    public async Task BulkCreateOrUpdateAsync(IReadOnlyList<AggregationGroup> groups, CancellationToken ct)
    {
        if (groups.Count == 0) return;

        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            await context.BulkInsertOrUpdateAsync(groups, config =>
            {
                config.PreserveInsertOrder = true;
                config.ConflictOption = ConflictOption.Replace;
                config.BatchSize = 8_000;
                config.UseTempDB = false;
                config.SetOutputIdentity = false;
                config.BulkCopyTimeout = 300;
            },cancellationToken: ct);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    
    public async Task<List<AggregationGroup>> GetByOperationNumbersAsync(
        IEnumerable<string> operationNumbers, CancellationToken ct)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        
        var numbers = operationNumbers.Distinct().ToList();
        return await context.AggregationGroups
            .Where(g => numbers.Contains(g.OperationNumber))
            .AsNoTracking()
            .ToListAsync(ct);
    }
}