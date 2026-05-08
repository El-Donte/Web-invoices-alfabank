using AbsIntegrationService.Models.DTOs;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IRawTransactionRepository
{
    Task<int> AddRangeAsync(IReadOnlyList<RawTransaction> transactions, CancellationToken ct = default);

    Task<List<RawTransactionDTO>> GetUngroupedTransactionsAsync(int batchSize, CancellationToken ct = default);
    Task AddAggregationGroupIdAsync(List<Guid> transactionIds, Guid aggregationGroupId, CancellationToken ct = default);

    Task UpdateStatusAsync(IReadOnlyList<Guid> ids, TransactionStatus status, string? error = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetExistingIdsByHashAsync(IReadOnlyList<string> hashes, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetExistingHashesAsync(IReadOnlyList<string> hashes, CancellationToken ct = default);
}