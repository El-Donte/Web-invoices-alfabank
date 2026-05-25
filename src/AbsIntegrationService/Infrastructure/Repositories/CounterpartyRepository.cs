using AbsIntegrationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class CounterpartyRepository(IDbContextFactory<AppDbContext> ctxFactory) : ICounterpartyRepository
{
    public async Task<Guid> GetCounterpartyIdByInnAsync(string inn, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            var entity = await context.Counterparties
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Inn == inn, ct);

            return entity?.Id ?? Guid.Empty;
        }
        catch (DbUpdateException ex)
        {
            throw ex;
        }
    }
    
    public async Task<List<(string Inn, Guid Id)>> GetAllCounterpartiesAsync()
    {
        await using var context = await ctxFactory.CreateDbContextAsync();

        try
        {
            return await context.Counterparties
                .Where(c => !string.IsNullOrEmpty(c.Inn))
                .Select(c => new { c.Inn, c.Id })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.Inn, x.Id)).ToList());
        }
        catch (DbUpdateException ex)
        {
            throw ex;
        }
    }
}