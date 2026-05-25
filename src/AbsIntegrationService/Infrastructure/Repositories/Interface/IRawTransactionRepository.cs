using AbsIntegrationService.Models.DTOs;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IRawTransactionRepository
{
    Task<int> AddRangeAsync(IReadOnlyList<RawTransaction> transactions, CancellationToken ct = default);
    Task<List<RawTransactionDTO>> GetUngroupedTransactionsAsync(int batchSize, CancellationToken ct = default);
    Task AddAggregationGroupIdAsync(List<Guid> transactionIds, Guid aggregationGroupId, CancellationToken ct = default);
}