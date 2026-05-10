using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class RawTransactionRepository(AppDbContext context) : IRawTransactionRepository
{
    public async Task<int> AddRangeAsync(IReadOnlyList<RawTransaction> transactions, CancellationToken ct = default)
    {
        if (transactions.Count == 0) return 0;
        
        context.RawTransactions.AddRange(transactions);
        await context.SaveChangesAsync(ct);
    
        return transactions.Count;
    }

    public async Task<List<RawTransactionDTO>> GetUngroupedTransactionsAsync(int batchSize, CancellationToken ct = default)
    {
        if (batchSize <= 0) return [];
        
        var ungrouped = await context.RawTransactions
            .Where(t => t.Status == TransactionStatus.Processed && t.AggregationGroupId == null)
            .OrderBy(t => t.ReceivedAt)
            .Take(batchSize)
            .Select(t => new RawTransactionDTO
            { 
                Id = t.Id, 
                OperationNumber = t.OperationNumber, 
                TransactionDate = t.Date.Value,
                Type = t.Type, 
                Amount = t.Amount, 
                NdsAmount = t.NdsAmount,
                DepartmentId = t.DepartmentId.Value,
                CounterpartyId = t.CounterpartyId.Value
            })
            .ToListAsync(ct);

        return ungrouped;
    }

    public async Task AddAggregationGroupIdAsync(List<Guid> transactionIds, Guid aggregationGroupId, CancellationToken ct = default)
    {
        if (transactionIds.Count == 0) return;
        if (aggregationGroupId == Guid.Empty) return;
        
        await context.RawTransactions
            .Where(t => transactionIds.Contains(t.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.AggregationGroupId, aggregationGroupId)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
    }
    
    public async Task UpdateStatusAsync(IReadOnlyList<Guid> ids, TransactionStatus status, string? error = null, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;
        
        await context.RawTransactions
            .Where(t => ids.Contains(t.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Status, status)
                .SetProperty(t => t.ValidationError, error)
                .SetProperty(t => t.ProcessedAt, DateTime.UtcNow), ct);
    }

    public async Task<IReadOnlyList<Guid>> GetExistingIdsByHashAsync(IReadOnlyList<string> hashes, CancellationToken ct = default)
    {
        if (hashes.Count == 0) return [];
        
        return await context.RawTransactions
            .AsNoTracking()
            .Where(t => hashes.Contains(t.PayloadHash))
            .Select(t => t.Id)
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<string>> GetExistingHashesAsync(IReadOnlyList<string> hashes, CancellationToken ct = default)
    {
        if (hashes.Count == 0) return [];
        
        return await context.RawTransactions
            .AsNoTracking()
            .Where(t => hashes.Contains(t.PayloadHash))
            .Select(t => t.PayloadHash)
            .ToListAsync(ct);
    }
}