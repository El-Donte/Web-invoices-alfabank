using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;
using AbsIntegrationService.Services.Kafka;

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
        var flushInterval = TimeSpan.FromMilliseconds(settings.Value.EventFlushIntervalMs);
        var batch = new List<AggregationReadyEvent>(batchSize);
        var reader = eventChannel.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool canRead = await reader.WaitToReadAsync(stoppingToken);
                if (!canRead) break;
                
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(flushInterval);

                try
                {
                    while (batch.Count < batchSize)
                    {
                        if (reader.TryRead(out var evt))
                        {
                            batch.Add(evt);
                        }
                        else
                        {
                            await reader.WaitToReadAsync(cts.Token).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {

                }

                if (batch.Count > 0)
                {
                    var toSend = batch.ToList();
                    batch.Clear();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await producer.ProduceBatchAggregationEventAsync(toSend, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to publish batch of {Count} events", toSend.Count);
                        }
                    }, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
        
        if (reader.TryPeek(out _))
        {
            var remaining = new List<AggregationReadyEvent>();
            while (reader.TryRead(out var evt))
                remaining.Add(evt);

            if (remaining.Count > 0)
                await producer.ProduceBatchAggregationEventAsync(remaining, CancellationToken.None);
        }
    }
}