using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Metrics;
using AbsIntegrationService.Models.DTOs;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;
using Shared.Entities;

namespace AbsIntegrationService.Services;

public class AggregationService(
    IServiceScopeFactory scopeFactory,
    Channel<AggregationReadyEvent> eventChannel,
    IOptions<AggregationWorkerSettings> settings,
    ILogger<AggregationService> logger)
    : IAggregationService
{
    private readonly AggregationWorkerSettings _settings = settings.Value;
    private static readonly TimeSpan SaveTimeout = TimeSpan.FromSeconds(30);

    public async Task<int> ProcessPendingBatchesAsync(CancellationToken ct = default)
    {
        using var readScope = scopeFactory.CreateScope();
        var rawTxRepo = readScope.ServiceProvider.GetRequiredService<IRawTransactionRepository>();
        
        var ungrouped = await rawTxRepo.GetUngroupedTransactionsAsync(_settings.BatchSize, ct);
        if (ungrouped.Count == 0)
            return 0;

        var groups = ungrouped
            .GroupBy(t => new AggregationRecord(t.OperationNumber, t.TransactionDate))
            .ToList();
        
        var existingGroupsDict = await LoadExistingGroupsAsync(groups, ct);
        
        var saveItems = new ConcurrentBag<AggregationSaveItem>();
        await Parallel.ForEachAsync(groups,
            new ParallelOptions { MaxDegreeOfParallelism = _settings.MaxParallelGroups, CancellationToken = ct },
            async (group, innerCt) =>
            {
                var item = await ProcessGroupAsync(group, existingGroupsDict, innerCt);
                if (item != null)
                    saveItems.Add(item);
            });
        
        await SaveAllAsync(saveItems, ct);

        return groups.Count;
    }

    private async Task<ConcurrentDictionary<string, AggregationGroup>> LoadExistingGroupsAsync(
        List<IGrouping<AggregationRecord, RawTransactionDTO>> groups,
        CancellationToken ct)
    {
        var operationNumbers = groups
            .Select(g => g.Key.OperationNumber)
            .Distinct()
            .ToList();

        using var scope = scopeFactory.CreateScope();
        var aggregationRepo = scope.ServiceProvider.GetRequiredService<IAggregationRepository>();
        var existing = await aggregationRepo.GetByOperationNumbersAsync(operationNumbers, ct);

        var dict = new ConcurrentDictionary<string, AggregationGroup>(
            existing.Select(g => new KeyValuePair<string, AggregationGroup>(g.OperationNumber, g)));
        return dict;
    }

    private async Task<AggregationSaveItem?> ProcessGroupAsync(  // переименуй для ясности
        IGrouping<AggregationRecord, RawTransactionDTO> group,
        ConcurrentDictionary<string, AggregationGroup> existingGroups,
        CancellationToken ct)
    {
        var operationNumber = group.Key.OperationNumber;
        var txIds = group.Select(t => t.Id).ToList();

        var sw = Stopwatch.StartNew();
        try
        {
            if (!existingGroups.TryGetValue(operationNumber, out var aggGroup))
            {
                aggGroup = CreateNewAggregationGroup(group);
            }

            UpdateCounts(aggGroup, group);
            aggGroup.LastProcessedAt = DateTime.UtcNow;

            if (ShouldMarkAsReady(aggGroup) && aggGroup.ReadyAt == null)
            {
                var pubSw = Stopwatch.StartNew();
                IngestionMetrics.RecordAggregationGroup();

                aggGroup.Status = AggregationStatus.Ready;
                aggGroup.ReadyAt = DateTime.UtcNow;
            
                var evt = CreateAggregationReadyEvent(aggGroup);
                await eventChannel.Writer.WriteAsync(evt, ct);  // теперь разрешено
                
                aggGroup.Status = AggregationStatus.Open;

                pubSw.Stop();
                IngestionMetrics.RecordEventPublishDuration(pubSw.Elapsed.TotalSeconds);
            }

            sw.Stop();
            IngestionMetrics.RecordAggregationGroupDuration(sw.Elapsed.TotalSeconds);

            return new AggregationSaveItem(aggGroup, txIds);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to process group {OperationNumber}", operationNumber);
            return null;
        }
    }

    private async Task SaveAllAsync(IEnumerable<AggregationSaveItem> items, CancellationToken ct)
    {
        var dict = new Dictionary<Guid, (AggregationGroup Group, HashSet<Guid> TxIds)>();
        foreach (var item in items)
        {
            if (dict.TryGetValue(item.Group.Id, out var existing))
            {
                foreach (var txId in item.TxIds) existing.TxIds.Add(txId);
            }
            else
            {
                dict[item.Group.Id] = (item.Group, [.. item.TxIds]);
            }
        }

        var allGroups = dict.Values.Select(v => v.Group).ToList();
        var allLinks = dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.TxIds.ToList());
        if (allGroups.Count == 0) return;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(SaveTimeout);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var aggregationRepo = scope.ServiceProvider.GetRequiredService<IAggregationRepository>();
            var rawTxRepo = scope.ServiceProvider.GetRequiredService<IRawTransactionRepository>();

            await aggregationRepo.BulkCreateOrUpdateAsync(allGroups, timeoutCts.Token);
            await rawTxRepo.AddAggregationGroupIdsBulkAsync(allLinks, timeoutCts.Token);

            IngestionMetrics.RecordMessage("aggregation_batch_save", allGroups.Count);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogError("Save timed out after {Timeout}s", SaveTimeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save aggregation batch");
        }
    }

    // Вспомогательные методы без изменений
    private static AggregationGroup CreateNewAggregationGroup(IGrouping<AggregationRecord, RawTransactionDTO> group) => new()
    {
        Id = Guid.NewGuid(),
        OperationNumber = group.Key.OperationNumber,
        TransactionDate = group.Key.TransactionDate,
        Status = AggregationStatus.Open,
        DepartmentId = group.FirstOrDefault(t => t.DepartmentId != null)?.DepartmentId,
        CounterpartyId = group.FirstOrDefault(t => t.CounterpartyId != null)?.CounterpartyId,
        LastProcessedAt = DateTime.UtcNow
    };

    private static void UpdateCounts(AggregationGroup agg, IEnumerable<RawTransactionDTO> txs)
    {
        agg.ShipmentCount += txs.Count(t => t.Type == TransactionType.Shipment);
        agg.AdvanceCount += txs.Count(t => t.Type == TransactionType.Advance);
        agg.CorrectiveCount += txs.Count(t => t.Type == TransactionType.Corrective);
        agg.TotalCount = agg.ShipmentCount + agg.AdvanceCount + agg.CorrectiveCount;
    }

    private bool ShouldMarkAsReady(AggregationGroup g) =>
        (g.TotalCount >= _settings.MinimumTransactionsForReady || IsTimeThresholdPassed(g))
        && g.ReadyAt == null;

    private bool IsTimeThresholdPassed(AggregationGroup g) =>
        g.LastProcessedAt.HasValue
        && (DateTime.UtcNow - g.LastProcessedAt.Value).TotalMinutes >= _settings.TimeoutMinutes;

    private static AggregationReadyEvent CreateAggregationReadyEvent(AggregationGroup g) => new(
        g.Id,
        g.OperationNumber,
        g.TransactionDate!.Value,
        g.ReadyAt!.Value,
        g.ShipmentCount,
        g.AdvanceCount,
        g.CorrectiveCount,
        g.TotalCount);
}

internal record AggregationSaveItem(AggregationGroup Group, IReadOnlyList<Guid> TxIds);