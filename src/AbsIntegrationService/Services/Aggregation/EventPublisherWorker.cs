using AbsIntegrationService.Services.Kafka;

using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;

namespace AbsIntegrationService.Services.Aggregation;

public class EventPublisherWorker(
    Channel<AggregationReadyEvent> eventChannel,
    IAggregationReadyEventProducer producer,
    IOptions<AggregationWorkerSettings> settings,
    ILogger<EventPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchSize = settings.Value.EventBatchSize;
        var flushIntervalMs = settings.Value.EventFlushIntervalMs;
        var batch = new List<AggregationReadyEvent>(batchSize);
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(flushIntervalMs));

        while (!stoppingToken.IsCancellationRequested)
        {
            bool tick = await timer.WaitForNextTickAsync(stoppingToken);
            if (!tick) break;
            
            while (batch.Count < batchSize && eventChannel.Reader.TryRead(out var evt))
                batch.Add(evt);

            if (batch.Count > 0)
            {
                try
                {
                    await producer.ProduceBatchAggregationEventAsync(batch, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish batch of {Count} events", batch.Count);
                }
                finally
                {
                    batch.Clear();
                }
            }
        }

        if (batch.Count > 0 || eventChannel.Reader.TryPeek(out _))
            await DrainAndPublishAsync(stoppingToken);
    }

    private async Task DrainAndPublishAsync(CancellationToken ct)
    {
        var remaining = new List<AggregationReadyEvent>();
        while (eventChannel.Reader.TryRead(out var evt))
            remaining.Add(evt);

        if (remaining.Count > 0)
            await producer.ProduceBatchAggregationEventAsync(remaining, ct);
    }
}