using System.Data;
using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Models.DTOs;
using Dapper;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Entities;
using ConflictOption = EFCore.BulkExtensions.ConflictOption;

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
        var connection = context.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        var sql = @"
        SELECT r.""Id"", 
               r.operation_number AS ""OperationNumber"", 
               r.date AS ""TransactionDate"", 
               r.type AS ""Type"", 
               r.amount AS ""Amount"", 
               r.nds_amount AS ""NdsAmount"", 
               r.department_id AS ""DepartmentId"", 
               r.counterparty_id AS ""CounterpartyId""
        FROM public.raw_transaction AS r
        WHERE r.status = 'Processed' AND r.aggregation_group_id IS NULL
        ORDER BY r.received_at
        LIMIT @batchSize
        FOR UPDATE SKIP LOCKED";

        var result = await connection.QueryAsync<RawTransactionDTO>(sql, new { batchSize });
        return result.AsList();
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

    public async Task AddAggregationGroupIdsBulkAsync(IDictionary<Guid, List<Guid>> groupToTxIds, CancellationToken ct)
    {
        if (groupToTxIds == null || groupToTxIds.Count == 0)
            return;
        
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        
        if (groupToTxIds == null || groupToTxIds.Count == 0)
            return;
        
        var rows = new List<(Guid TxId, Guid GroupId)>();
        foreach (var (groupId, txIds) in groupToTxIds)
        {
            rows.AddRange(txIds.Select(txId => (txId, groupId)));
        }
        
        const int chunkSize = 8000;
        for (int i = 0; i < rows.Count; i += chunkSize)
        {
            var chunk = rows.Skip(i).Take(chunkSize).ToList();
            
            var valuesList = new List<string>();
            var parameters = new List<NpgsqlParameter>();
            int idx = 0;
            foreach (var (txId, groupId) in chunk)
            {
                var txParam = new NpgsqlParameter($"@t{idx}", txId);
                var grParam = new NpgsqlParameter($"@g{idx}", groupId);
                parameters.Add(txParam);
                parameters.Add(grParam);
                valuesList.Add($"(@t{idx}, @g{idx})");
                idx++;
            }

            var sql = $@"
            UPDATE ""raw_transaction"" AS rt
            SET ""aggregation_group_id"" = v.""GroupId""
            FROM (VALUES {string.Join(", ", valuesList)}) AS v(""Id"", ""GroupId"")
            WHERE rt.""Id"" = v.""Id""";

            await context.Database.ExecuteSqlRawAsync(sql, parameters, ct);
        }

    }
}