using System.Diagnostics;
using Confluent.Kafka;
using InvoicesWebService.Metrics;
using InvoicesWebService.Services.Interfaces;
using Messaging.Kafka;
using Messaging.Kafka.Consumer;
using Shared;
using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.Kafka;

public class AggregationReadyConsumer : KafkaConsumer<AggregationReadyEvent>
{
    public AggregationReadyConsumer(
        IServiceProvider serviceProvider,
        KafkaSettings settings,
        ILogger<AggregationReadyConsumer> logger,
        KafkaJsonDeserializer<AggregationReadyEvent>  deserializer
        ) : base(settings, serviceProvider, logger)
    {
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
        _logger.LogInformation("Draft consumer started. Topic: {Topic}", _topic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (result is null) continue;

                    await ConsumeAsync(result, stoppingToken);
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
                    _logger.LogError(ex, "Unexpected consume loop error.");
                }
            }
        }
        finally
        {
            _consumer?.Close();
            _logger.LogInformation("Kafka consumer stopped gracefully.");
            _consumer?.Dispose();
        }
    }

    private async Task ConsumeAsync(ConsumeResult<string, AggregationReadyEvent> result, CancellationToken ct)
    {
        var evt = result.Message.Value;

        using var scope = _serviceProvider.CreateScope();
        var draftService = scope.ServiceProvider.GetRequiredService<IDraftInvoiceService>();
        var errorService = scope.ServiceProvider.GetRequiredService<IProcessingErrorService>();
        
        var sw = Stopwatch.StartNew();
        try
        {
            await draftService.ProcessAggregationReadyAsync(evt, ct);
            
            _consumer.Commit(result);
            sw.Stop();
            InvoiceMetrics.RecordDraftDuration(sw.Elapsed.TotalSeconds, "success");
        }
        catch (Exception ex)
        {
            sw.Stop();
            InvoiceMetrics.RecordDraftDuration(sw.Elapsed.TotalSeconds, "failed");
            InvoiceMetrics.RecordDraftError(ex.GetType().Name);
            
            await errorService.LogAsync(new ErrorLogEntry(
                ProcessingStage.Creation, 
                "DRAFT_CREATION_ERROR", 
                ex.Message, 
                "", 
                true, 
                evt.AggregationGroupId), ct);
        }
    }
}