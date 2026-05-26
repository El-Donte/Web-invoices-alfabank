using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IAggregationRepository
{
    Task<AggregationGroup?> GetByOperationNumberAsync(string operationNumber, CancellationToken ct = default);
    Task CreateOrUpdateAsync(AggregationGroup group, CancellationToken ct = default);
    Task BulkCreateOrUpdateAsync(IReadOnlyList<AggregationGroup> groups, CancellationToken ct);
    Task<List<AggregationGroup>> GetByOperationNumbersAsync(
        IEnumerable<string> operationNumbers, CancellationToken ct);
}