using System.Diagnostics.Metrics;

namespace AbsIntegrationService.Metrics;

public static class IngestionMetrics
{
    private static readonly Meter Meter = new("AbsIntegrationService", "1.0.0");

    public static readonly Counter<long> KafkaMessagesProcessed = 
        Meter.CreateCounter<long>("kafka.messages.processed.total", "{message}", "Total Kafka messages processed");

    public static readonly Histogram<double> BatchProcessingDuration = 
        Meter.CreateHistogram<double>("kafka.batch.processing.duration", "s", "Batch processing duration");

    public static readonly Counter<long> DuplicateMessagesTotal = 
        Meter.CreateCounter<long>("ingestion.duplicate.messages.total", "{message}", "Idempotent duplicate drops");

    public static readonly Counter<long> ValidationErrorsTotal = 
        Meter.CreateCounter<long>("ingestion.validation.errors.total", "{error}", "Business validation failures");

    public static readonly Counter<long> AggregationGroupsProcessed = 
        Meter.CreateCounter<long>("aggregation.groups.processed.total", "{group}", "Aggregation groups transitioned to READY");
    
    public static void RecordMessage(string status) => 
        KafkaMessagesProcessed.Add(1, new KeyValuePair<string, object?>("status", status));

    public static void RecordBatchDuration(double seconds) => 
        BatchProcessingDuration.Record(seconds);

    public static void RecordDuplicate() => 
        DuplicateMessagesTotal.Add(1);

    public static void RecordValidationError(string rule) => 
        ValidationErrorsTotal.Add(1, new KeyValuePair<string, object?>("rule", rule));

    public static void RecordAggregationGroup() => 
        AggregationGroupsProcessed.Add(1);
}