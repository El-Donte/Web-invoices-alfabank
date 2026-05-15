using System.Diagnostics;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Metrics;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using AbsIntegrationService.Services.Kafka;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;
using Shared.Entities;
using System.Threading.Tasks.Dataflow;
using AbsIntegrationService.Models.DTOs;

namespace AbsIntegrationService.Services;

public class AggregationService(
    IServiceScopeFactory scopeFactory,
    IAggregationReadyEventProducer producer,
    IOptions<AggregationWorkerSettings> settings,
    ILogger<AggregationService> logger)
    : IAggregationService
{
    private readonly AggregationWorkerSettings _settings = settings.Value;

    public async Task<int> ProcessPendingBatchesAsync(CancellationToken ct = default)
    {
        using var readScope = scopeFactory.CreateScope();
        var rawTxRepo = readScope.ServiceProvider.GetRequiredService<IRawTransactionRepository>();

        var ungrouped = await rawTxRepo.GetUngroupedTransactionsAsync(_settings.BatchSize, ct);
        if (ungrouped.Count == 0) return 0;

        var groups = ungrouped
            .GroupBy(t => new AggregationRecord(t.OperationNumber, t.TransactionDate))
            .ToList();

        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 20,
            CancellationToken = ct,
            EnsureOrdered = false
        };

        var processor = new ActionBlock<IGrouping<AggregationRecord, RawTransactionDTO>>(
            group => ProcessGroupAsync(group, ct), 
            options);

        foreach (var group in groups)
        {
            await processor.SendAsync(group, ct);
        }

        processor.Complete();
        await processor.Completion;

        return groups.Count;
    }

    private async Task ProcessGroupAsync(IGrouping<AggregationRecord, RawTransactionDTO> group, CancellationToken ct)
    {
        var operationNumber = group.Key.OperationNumber;
        var txIds = group.Select(t => t.Id).ToList();

        var sw = Stopwatch.StartNew();

        try
        {
            using var scope = scopeFactory.CreateScope();
            var aggregationRepo = scope.ServiceProvider.GetRequiredService<IAggregationRepository>();
            var rawTxRepo = scope.ServiceProvider.GetRequiredService<IRawTransactionRepository>();

            var aggGroup = await aggregationRepo.GetByOperationNumberAsync(operationNumber, ct)
                           ?? CreateNewAggregationGroup(group);

            UpdateCounts(aggGroup, group);
            aggGroup.LastProcessedAt = DateTime.UtcNow;

            if (ShouldMarkAsReady(aggGroup) && aggGroup.ReadyAt == null)
            {
                var pubSw = Stopwatch.StartNew();
                IngestionMetrics.RecordAggregationGroup();

                aggGroup.Status = AggregationStatus.Ready;
                aggGroup.ReadyAt = DateTime.UtcNow;

                await aggregationRepo.CreateOrUpdateAsync(aggGroup, ct);
                await rawTxRepo.AddAggregationGroupIdAsync(txIds, aggGroup.Id, ct);

                var evt = CreateAggregationReadyEvent(aggGroup);
                await producer.ProduceAggregationEventAsync(evt, ct);
                
                aggGroup.Status = AggregationStatus.Open;
                await aggregationRepo.CreateOrUpdateAsync(aggGroup, ct);
                
                pubSw.Stop();
                IngestionMetrics.RecordEventPublishDuration(pubSw.Elapsed.TotalSeconds);
            }
            else
            {
                await aggregationRepo.CreateOrUpdateAsync(aggGroup, ct);
                await rawTxRepo.AddAggregationGroupIdAsync(txIds, aggGroup.Id, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process group {OperationNumber}", operationNumber);
        }
        finally
        {
            sw.Stop();
            IngestionMetrics.RecordAggregationGroupDuration(sw.Elapsed.TotalSeconds);
        }
    }

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
        g.TransactionDate.Value, 
        g.ReadyAt!.Value,
        g.ShipmentCount, 
        g.AdvanceCount, 
        g.CorrectiveCount, 
        g.TotalCount);
}