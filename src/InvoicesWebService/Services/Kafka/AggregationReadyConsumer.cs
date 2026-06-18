using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using InvoicesWebService.Metrics;
using InvoicesWebService.Services.Interfaces;
using Messaging.Kafka;
using Messaging.Kafka.Consumer;
using Shared;
using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.Kafka;

public sealed class AggregationReadyConsumer : KafkaConsumer<AggregationReadyEvent>
{
    public AggregationReadyConsumer(
        IServiceProvider serviceProvider,
        KafkaSettings settings,
        ILogger<AggregationReadyConsumer> logger,
        KafkaJsonDeserializer<AggregationReadyEvent> deserializer)
        : base(settings, serviceProvider, logger)
    {
        _config.FetchMinBytes = 1024;
        _config.FetchMaxBytes = 10_485_760;

        _consumer = new ConsumerBuilder<string, AggregationReadyEvent>(_config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason} | Code: {Code}", e.Reason, e.Code))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", 
                    string.Join(", ", partitions.Select(p => p.Partition.Value)));
            })
            .SetValueDeserializer(deserializer)
            .Build();

        _consumer.Subscribe(_topic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AggregationReadyConsumer started. Topic: {Topic}, Group: {Group}", 
            _topic, _settings.GroupId);

        var batch = new List<ConsumeResult<string, AggregationReadyEvent>>();
        var lastFlush = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (result is null)
                    {
                        if (batch.Count > 0 && 
                            (DateTime.UtcNow - lastFlush).TotalMilliseconds > _settings.FlushIntervalMs)
                        {
                            await FlushBatchAsync(batch, stoppingToken);
                            batch.Clear();
                            lastFlush = DateTime.UtcNow;
                        }
                        continue;
                    }

                    batch.Add(result);

                    if (batch.Count >= _settings.BatchSize)
                    {
                        await FlushBatchAsync(batch, stoppingToken);
                        batch.Clear();
                        lastFlush = DateTime.UtcNow;
                    }
                }
                catch (ConsumeException ex) when (ex.Error.IsFatal)
                {
                    _logger.LogCritical(ex, "Fatal Kafka error.");
                    break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in consume loop");
                }
            }

            if (batch.Count > 0)
                await FlushBatchAsync(batch, CancellationToken.None);
        }
        finally
        {
            _consumer?.Close();
            _logger.LogInformation("AggregationReadyConsumer stopped gracefully.");
            _consumer?.Dispose();
        }
    }

    private async Task FlushBatchAsync(List<ConsumeResult<string, AggregationReadyEvent>> batch, CancellationToken ct)
    {
        if (batch.Count == 0) return;

        using var scope = _serviceProvider.CreateScope();
        var draftService = scope.ServiceProvider.GetRequiredService<IDraftInvoiceCreationService>();
        var errorService = scope.ServiceProvider.GetRequiredService<IProcessingErrorService>();

        var sw = Stopwatch.StartNew();
        var processedCount = 0;

        if (batch.Count == 0)
        {
            CommitOffsets(batch);
            return;
        }
        
        try
        {
            try
            {
                await draftService.ProcessAggregationReadyAsync(batch, ct);
            }
            catch (Exception ex)
            {
                InvoiceMetrics.RecordDraftDuration(0, "failed");
                InvoiceMetrics.RecordDraftError(ex.GetType().Name);

                await errorService.LogAsync(new ErrorLogEntry(
                    ProcessingStage.Creation,
                    "BATCH_PROCESS ERROR",
                    ex.Message,
                    JsonSerializer.Serialize(batch.First().Message.Value),
                    true,
                    batch.First().Message.Value.AggregationGroupId), ct);
            }

            _logger.LogInformation("Processed batch of {Count} AggregationReady events", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AggregationReady batch");
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("AggregationReady batch processed in {Elapsed} sec", 
                sw.Elapsed.TotalSeconds);
            InvoiceMetrics.RecordDraftDuration(sw.Elapsed.TotalSeconds, "success");
        }
    }
    
    private void CommitOffsets(List<ConsumeResult<string, AggregationReadyEvent>> batch)
    {
        if (batch.Count == 0) return;
    
        var offsets = batch
            .AsParallel()
            .GroupBy(r => r.TopicPartition)
            .Select(g => new TopicPartitionOffset(g.Key, g.Max(r => r.Offset.Value) + 1));
    
        _consumer.Commit(offsets);
    }
}