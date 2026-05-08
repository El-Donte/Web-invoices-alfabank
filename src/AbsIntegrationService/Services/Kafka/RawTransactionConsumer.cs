using Shared.Contracts;
using AbsIntegrationService.Services.Interfaces;
using Confluent.Kafka;

namespace AbsIntegrationService.Services.Kafka;

public sealed class RawTransactionConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<RawTransactionConsumer> _logger;
    private readonly KafkaSettings _settings;
    private readonly JsonDeserializer<AbsMessage> _deserializer;
    
    private readonly string _topic;
    private bool _disposed;
    
    public RawTransactionConsumer(
        IServiceProvider serviceProvider,
        KafkaSettings settings,
        ILogger<RawTransactionConsumer> logger,
        JsonDeserializer<AbsMessage> deserializer
        )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _topic = settings.Topic;
        _logger = logger;
        _deserializer = deserializer;
        
        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = 300000,
            SessionTimeoutMs = 30000,
            FetchMinBytes = 1024,
            FetchMaxBytes = 10_485_760,
            MaxPartitionFetchBytes = 1048576,
            SocketTimeoutMs = 120000,
            HeartbeatIntervalMs = 3000
        };
        
        _consumer = new ConsumerBuilder<string, string>(config)
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
        _consumer.Subscribe(_settings.Topic);
        _logger.LogInformation("Kafka consumer started. Topic: {Topic}, Group: {Group}", _settings.Topic, _settings.GroupId);

        var batch = new List<ConsumeResult<string, string>>();
        var lastFlush = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                if (result is null)
                {
                    if (batch.Count > 0 && (DateTime.UtcNow - lastFlush).TotalMilliseconds > _settings.FlushIntervalMs)
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
                _logger.LogCritical(ex, "Fatal Kafka consumer error. Stopping.");
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in consume loop");
            }
        }
        
        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch, CancellationToken.None);
        }

        _consumer.Close();
        _logger.LogInformation("Kafka consumer stopped gracefully.");
    }

    private async Task FlushBatchAsync(List<ConsumeResult<string, string>> batch, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<ITransactionIngestionService>();
        var errorService = scope.ServiceProvider.GetRequiredService<IErrorHandlingService>();

        var parsedBatch =  new List<(string RawPayload, AbsMessage Message)>();
        
        foreach (var msg in batch)
        {
            try
            {
                var message = _deserializer.Deserialize(msg.Message.Value);
                parsedBatch.Add((msg.Message.Value, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize message. Offset will be committed to avoid poison pill.");
            }
        }
        
        try
        {
            var result = await ingestionService.ProcessBatchAsync(parsedBatch, ct);
            CommitOffsets(batch);

            _logger.LogInformation(
                "Batch processed. Total: {Total}, Inserted: {Inserted}, Duplicates: {Dups}, ValidationErrors: {Errs}",
                result.Total, result.Inserted, result.Duplicates, result.ValidationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB SaveChanges failed for batch. Offsets NOT committed. Kafka will redeliver.");
            
            foreach (var (payload, msg) in parsedBatch)
            {
                await errorService.LogErrorAsync("DB_SAVE_FAILED", ex.Message, payload, true, ct);
            }
        }
    }

    private void CommitOffsets(List<ConsumeResult<string, string>> batch)
    {
        var offsetsToCommit = batch
            .GroupBy(r => r.TopicPartition)
            .Select(g => new TopicPartitionOffset(g.Key, g.Max(r => r.Offset) + 1));
        
        _consumer.Commit(offsetsToCommit);
    }
}