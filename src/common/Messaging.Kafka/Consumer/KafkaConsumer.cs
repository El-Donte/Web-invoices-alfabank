using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Kafka.Consumer;

public class KafkaConsumer<TMessage> : BackgroundService
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger<KafkaConsumer<TMessage>> _logger;
    protected readonly KafkaSettings _settings;

    protected readonly ConsumerConfig _config;
    protected IConsumer<string, TMessage> _consumer;
    
    protected readonly string _topic;
    private bool _disposed;
    
    protected KafkaConsumer(KafkaSettings kafkaSettings, IServiceProvider serviceProvider,  ILogger<KafkaConsumer<TMessage>> logger)
    {
        _settings = kafkaSettings;
        _serviceProvider = serviceProvider;
        _topic = kafkaSettings.Topic;
        _logger = logger;
        
        _config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            
            FetchMinBytes = 1,
            MaxPartitionFetchBytes = 1_048_576,

            MaxPollIntervalMs = 300_000,
            SessionTimeoutMs = 30_000,
            HeartbeatIntervalMs = 3_000,
            SocketTimeoutMs = 120_000,

            EnablePartitionEof = false,
            IsolationLevel = IsolationLevel.ReadCommitted
        };
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(100));

                    if (consumeResult == null || consumeResult.IsPartitionEOF)
                        continue;
                    
                    _consumer.Commit(consumeResult);
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
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        if (!_disposed)
        {
            _consumer?.Dispose();
            _disposed = true;
        }
        base.Dispose();
    }
}