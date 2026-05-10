using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using AbsIntegrationService.Services.Kafka;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;
using Shared.Entities;

namespace AbsIntegrationService.Services;

public class AggregationService(IAggregationRepository aggregationRepository, 
    IRawTransactionRepository rawTransactionRepository,
    IAggregationReadyEventProducer producer,
    IOptions<AggregationWorkerSettings> settings)
    : IAggregationService
{

    private readonly AggregationWorkerSettings _settings = settings.Value;
    public async Task<int> ProcessPendingBatchesAsync(CancellationToken ct = default)
    {
        var ungrouped = await rawTransactionRepository.GetUngroupedTransactionsAsync(_settings.BatchSize, ct);

        if (ungrouped.Count == 0) return 0;
        
        var groups = ungrouped.GroupBy(t => t.OperationNumber);
        var processedGroupCount = 0;

        foreach (var group in groups)
        {
            var operationNumber = group.Key;
            var txIds = group.Select(t => t.Id).ToList();
            
            var aggGroup = await aggregationRepository.GetByOperationNumberAsync(operationNumber, ct) 
                           ?? new AggregationGroup
            {
                Id = Guid.NewGuid(),
                OperationNumber = operationNumber,
                TransactionDate = DateTime.UtcNow.Date,
                Status = AggregationStatus.Open,
                ShipmentCount = 0,
                AdvanceCount = 0,
                CorrectiveCount = 0,
                TotalCount = 0,
                ReadyAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await rawTransactionRepository.AddAggregationGroupIdAsync(txIds, aggGroup.Id, ct);
            
            aggGroup.ShipmentCount += group.Count(t => t.Type == TransactionType.Shipment);
            aggGroup.AdvanceCount += group.Count(t => t.Type == TransactionType.Advance);
            aggGroup.CorrectiveCount += group.Count(t => t.Type == TransactionType.Corrective);
            aggGroup.TotalCount = group.Count();
            aggGroup.UpdatedAt = DateTime.UtcNow;

            if (aggGroup.TotalCount >= _settings.MinimumTransactionsForReady || IsTimeThresholdPassed(aggGroup))
            {
                aggGroup.Status = AggregationStatus.Ready;
                aggGroup.ReadyAt = DateTime.UtcNow;
            }
            

            if (aggGroup.Status == AggregationStatus.Ready || aggGroup.ReadyAt.HasValue)
            {
                var evt = new AggregationReadyEvent(aggGroup.Id, aggGroup.OperationNumber, 
                    aggGroup.ReadyAt, aggGroup.ShipmentCount, 
                    aggGroup.AdvanceCount, aggGroup.CorrectiveCount, aggGroup.TotalCount);
                
                producer.ProduceAggregationEventAsync(evt, ct);
                
            //     context.OutboxEvents.Add(new OutboxEvent
            //     {
            //         Id = Guid.NewGuid(),
            //         EventType = nameof(AggregationReadyEvent),
            //         Payload = JsonSerializer.Serialize(evt),
            //         Status = OutboxStatus.Pending,
            //         CreatedAt = DateTime.UtcNow
            //     });
            }

            processedGroupCount++;
        }
        
        return processedGroupCount;
    }

    private bool IsTimeThresholdPassed(AggregationGroup group) =>
        group.LastProcessedAt.HasValue 
        && (DateTime.UtcNow - group.LastProcessedAt.Value).TotalMinutes >= _settings.TimeoutMinutes;
}