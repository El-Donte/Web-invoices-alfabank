using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Models.DTOs;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class RawTransactionRepository(IDbContextFactory<AppDbContext> ctxFactory, ILogger<RawTransactionRepository> _logger) : IRawTransactionRepository
{
    public async Task<int> AddRangeAsync(IReadOnlyList<RawTransaction> transactions, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            if (transactions.Count == 0) return 0;
            
            await context.BulkInsertAsync(transactions, config =>
            {
                config.PreserveInsertOrder = true;
                config.SetOutputIdentity = false;
                config.ConflictOption = ConflictOption.Ignore;
                config.BatchSize = 8_000;
                config.EnableStreaming = true;
                config.UseTempDB = false;
                config.BulkCopyTimeout = 300;
            }, cancellationToken: ct);
            
            
            return transactions.Count;
        
        }
        catch (DbUpdateException ex)
        {
            return 0;
        }
        
    }

    public async Task<List<RawTransactionDTO>> GetUngroupedTransactionsAsync(int batchSize, CancellationToken ct = default)
    {
        if (batchSize <= 0) return [];
        
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            var ungrouped = await context.RawTransactions
                .AsNoTracking()
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
        catch (DbUpdateException ex)
        {
            throw ex;
        }
    }

    public async Task AddAggregationGroupIdAsync(List<Guid> transactionIds, Guid aggregationGroupId, CancellationToken ct = default)
    {
        if (transactionIds.Count == 0) return;
        if (aggregationGroupId == Guid.Empty) return;
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            await context.RawTransactions
                .Where(t => transactionIds.Contains(t.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.AggregationGroupId, aggregationGroupId)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
        }
        catch (DbUpdateException ex)
        {
            throw ex;
        }
    }
}