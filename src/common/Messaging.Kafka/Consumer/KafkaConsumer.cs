using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messaging.Kafka.Consumer;

public class KafkaConsumer<TMessage> : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, TMessage> _consumer;
    private readonly ILogger<KafkaConsumer<TMessage>> _logger;
    
    private readonly string _topic;
    private bool _disposed;
    
    public KafkaConsumer(IOptions<KafkaSettings> kafkaSettings, IServiceProvider serviceProvider,  ILogger<KafkaConsumer<TMessage>> logger)
    {
        _serviceProvider = serviceProvider;
        _topic = kafkaSettings.Value.Topic;
        _logger = logger;
        
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
            GroupId =  kafkaSettings.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false,
            FetchMaxBytes = 10485760,                   // 10 MB
            MaxPartitionFetchBytes = 1048576,
            MaxPollIntervalMs = 300000,  
            SocketTimeoutMs = 120000,
            SessionTimeoutMs = 45000,
            HeartbeatIntervalMs = 3000
        };
        
        _consumer = new ConsumerBuilder<string, TMessage>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason} | Code: {Code}", e.Reason, e.Code))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", 
                    string.Join(", ", partitions.Select(p => p.Partition.Value)));
            })
            .SetValueDeserializer(new KafkaJsonDeserializer<TMessage>())
            .Build();
        
        _consumer.Subscribe(_topic);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult == null || consumeResult.IsPartitionEOF)
                        continue;

                    await ConsumeAsync(consumeResult, stoppingToken);
                    
                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Consume error");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consumer loop");
                    break;
                }
            }
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }

    private async Task ConsumeAsync(ConsumeResult<string, TMessage> result, CancellationToken stoppingToken)
    {
        var message = result.Message.Value;
        var key = result.Message.Key;

        _logger.LogDebug("Received message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}",
            result.Topic, result.Partition, result.Offset, key);

        using var scope = _serviceProvider.CreateScope();
        
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

        try
        {
            await handler.HandleAsync(message, stoppingToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for message key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message with key: {Key}", key);
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