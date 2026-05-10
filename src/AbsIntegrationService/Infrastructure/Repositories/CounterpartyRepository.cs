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
}