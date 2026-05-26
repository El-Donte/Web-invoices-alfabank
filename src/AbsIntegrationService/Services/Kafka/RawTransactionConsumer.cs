using System.Collections.Concurrent;
using System.Diagnostics;
using AbsIntegrationService.Metrics;
using Shared.Contracts;
using AbsIntegrationService.Services.Interfaces;
using Shared.Metrics;
using Confluent.Kafka;
using Messaging.Kafka;
using Messaging.Kafka.Consumer;
using Shared;
using Shared.Entities;

namespace AbsIntegrationService.Services.Kafka;

public sealed class RawTransactionConsumer : KafkaConsumer<string>
{
    private readonly KafkaJsonDeserializer<AbsMessage> _deserializer;
    
    public RawTransactionConsumer(
        IServiceProvider serviceProvider,
        KafkaSettings settings,
        KafkaJsonDeserializer<AbsMessage> deserializer,
        ILogger<RawTransactionConsumer> logger
        ) : base(settings, serviceProvider, logger)
    {
        _deserializer = deserializer;

        _config.FetchMinBytes = 1024;
        _config.FetchMaxBytes = 10_485_760;
        
        _consumer = new ConsumerBuilder<string, string>(_config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason} | Code: {Code}", e.Reason, e.Code))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", 
                    string.Join(", ", partitions.Select(p => p.Partition.Value)));
            })
            .Build();
        
        _consumer.Subscribe(_topic);
    }
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka consumer started. Topic: {Topic}, Group: {Group}", _topic, _settings.GroupId);

        var batch = new List<ConsumeResult<string, string>>();
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
                    IngestionMetrics.RecordMessage("CRITICAL_ERROR");
                    break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    IngestionMetrics.RecordMessage("ERROR");
                }
            }
            
            if (batch.Count > 0)
            {
                await FlushBatchAsync(batch, CancellationToken.None);
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped gracefully.");
            _consumer.Dispose();
        }
    }

    private async Task FlushBatchAsync(List<ConsumeResult<string, string>> batch, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<ITransactionIngestionService>();
        var errorService = scope.ServiceProvider.GetRequiredService<IProcessingErrorService>();
        
        var sw = Stopwatch.StartNew();
        var parsedBatch = new ConcurrentBag<(string RawPayload, AbsMessage Message)>();

        await Parallel.ForEachAsync(batch, new ParallelOptions 
        { 
            MaxDegreeOfParallelism = 8,
            CancellationToken = ct 
        }, async (msg, token) =>
        {
            try
            {
                var message = _deserializer.Deserialize(msg.Message.Value);
                parsedBatch.Add((msg.Message.Value, message));
            }
            catch (Exception ex)
            {
                IngestionMetrics.RecordMessage("DESERIALIZATION_FAILURE");
                _logger.LogError(ex, "Deserialization failed");
            }
        });

        if (parsedBatch.IsEmpty)
        {
            CommitOffsets(batch);
            return;
        }

        try
        {
            var result = await ingestionService.ProcessBatchAsync(parsedBatch.ToList(), ct);

            CommitOffsets(batch);

            IngestionMetrics.RecordMessage("success", result.Total);
            if (result.ValidationErrors > 0) IngestionMetrics.RecordValidationError("business_rules");
            if (result.Duplicates > 0) IngestionMetrics.RecordDuplicate();
        }
        catch (Exception ex)
        {
            IngestionMetrics.RecordMessage("db_error", batch.Count);
            AppMetrics.RecordDbError("SaveChanges", ex.GetType().Name);

            foreach (var (payload, msg) in parsedBatch)
            {
                await errorService.LogAsync(new ErrorLogEntry(ProcessingStage.Ingest, "DB_SAVE_FAILED",
                    ex.Message, payload, Retryable: true), ct);
            }
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation($"Raw_transaction {sw.Elapsed.TotalSeconds} seconds elapsed.");
            IngestionMetrics.RecordRawTransactionDuration(sw.Elapsed.TotalSeconds);
        }
    }

    private void CommitOffsets(List<ConsumeResult<string, string>> batch)
    {
        if (batch.Count == 0) return;
    
        var offsets = batch
            .AsParallel()
            .GroupBy(r => r.TopicPartition)
            .Select(g => new TopicPartitionOffset(g.Key, g.Max(r => r.Offset.Value) + 1));
    
        _consumer.Commit(offsets);
    }
}