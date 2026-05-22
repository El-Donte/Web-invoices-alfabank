using AbsIntegrationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class CounterpartyRepository(AppDbContext context) : ICounterpartyRepository
{
    public async Task<Guid> GetCounterpartyIdByInnAsync(string inn, CancellationToken cancellationToken = default)
    {
        var entity = await context.Counterparties
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Inn == inn, cancellationToken);
        
        return entity?.Id ?? Guid.Empty;
    }
    
    public async Task<Dictionary<string, Guid>> GetCounterpartyIdsByInnBatchAsync(
        List<string> inns, CancellationToken ct = default)
    {
        if (inns == null || inns.Count == 0) 
            return new Dictionary<string, Guid>();

        return await context.Counterparties
            .Where(c => inns.Contains(c.Inn))
            .ToDictionaryAsync(c => c.Inn, c => c.Id, ct);
    }
}