using System.Diagnostics.Metrics;

namespace AbsIntegrationService.Metrics;

public static class IngestionMetrics
{
    private static readonly Meter Meter = new("AbsIntegrationService", "1.0.0");

    public static readonly Counter<long> MessagesProcessed = Meter.CreateCounter<long>(
        "invoice.messages_processed_total", 
        description: "Total messages processed");
    
    public static readonly Counter<long> DuplicateMessagesTotal = 
        Meter.CreateCounter<long>("ingestion.duplicate.messages.total", "{message}", "Idempotent duplicate drops");

    public static readonly Counter<long> ValidationErrorsTotal = 
        Meter.CreateCounter<long>("ingestion.validation.errors.total", "{error}", "Business validation failures");

    public static readonly Counter<long> AggregationGroupsProcessed = 
        Meter.CreateCounter<long>("aggregation.groups.processed.total", "{group}", "Aggregation groups transitioned to READY");
    
    public static readonly Histogram<double> RawTransactionCreationDuration = 
        Meter.CreateHistogram<double>("ingestion.raw_transaction.creation.duration", "s", "Time to create and persist one RawTransaction");

    public static readonly Histogram<double> AggregationGroupCreationDuration = 
        Meter.CreateHistogram<double>("aggregation.group.creation.duration", "s", "Time to create/update one AggregationGroup");

    public static readonly Histogram<double> AggregationEventPublishDuration = 
        Meter.CreateHistogram<double>("aggregation.event.publish.duration", "s", "Time from group readiness to Kafka event publish");

    public static void RecordRawTransactionDuration(double seconds) => 
        RawTransactionCreationDuration.Record(seconds);
    
    public static void RecordAggregationGroupDuration(double seconds) => 
        AggregationGroupCreationDuration.Record(seconds);
    
    public static void RecordEventPublishDuration(double seconds) => 
        AggregationEventPublishDuration.Record(seconds);
    
    public static void RecordMessage(string status,int count = 1) => 
        MessagesProcessed.Add(count);
    

    public static void RecordDuplicate() => 
        DuplicateMessagesTotal.Add(1);

    public static void RecordValidationError(string rule) => 
        ValidationErrorsTotal.Add(1, new KeyValuePair<string, object?>("rule", rule));

    public static void RecordAggregationGroup() => 
        AggregationGroupsProcessed.Add(1);
}