using System.Diagnostics.Metrics;

namespace App.Metrics;

public static class AppMetrics
{
    public static readonly Meter Meter = new("InvoiceSystem", "1.0.0");
    
    public static readonly Counter<long> KafkaMessagesProcessed = 
        Meter.CreateCounter<long>("kafka.messages.processed.total", "{message}", "Total Kafka messages processed");

    public static readonly Histogram<double> BatchProcessingDuration = 
        Meter.CreateHistogram<double>("kafka.batch.processing.duration", "s", "Batch processing duration");
    
    public static readonly Histogram<double> DbOperationDuration = 
        Meter.CreateHistogram<double>("db.operation.duration", "s", "Database operation duration");

    public static readonly Counter<long> DbErrorsTotal = 
        Meter.CreateCounter<long>("db.errors.total", "{error}", "Database execution errors");
    
    public static void RecordMessage(string status) => 
        KafkaMessagesProcessed.Add(1, new KeyValuePair<string, object?>("status", status));

    public static void RecordBatchDuration(double seconds) => 
        BatchProcessingDuration.Record(seconds);

    public static void RecordDbOperation(string operation, double seconds) => 
        DbOperationDuration.Record(seconds, new KeyValuePair<string, object?>("operation", operation));

    public static void RecordDbError(string operation, string type) => 
        DbErrorsTotal.Add(1, 
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("error_type", type));
}