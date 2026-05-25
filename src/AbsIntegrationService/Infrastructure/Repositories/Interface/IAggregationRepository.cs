using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IAggregationRepository
{
    Task<AggregationGroup?> GetByOperationNumberAsync(string operationNumber, CancellationToken ct = default);
    Task CreateOrUpdateAsync(AggregationGroup group, CancellationToken ct = default);
}