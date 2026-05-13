using System.Diagnostics.Metrics;

namespace Shared.Metrics;

public static class AppMetrics
{
    public static readonly Meter Meter = new("InvoiceSystem", "1.0.0");
    
    public static readonly Histogram<double> DbOperationDuration = 
        Meter.CreateHistogram<double>("db.operation.duration", "s", "Database operation duration");
    public static readonly Counter<long> DbErrorsTotal = 
        Meter.CreateCounter<long>("db.errors.total", "{error}", "Database execution errors");
    
    public static void RecordDbOperation(string operation, double seconds) => 
        DbOperationDuration.Record(seconds, new KeyValuePair<string, object?>("operation", operation));

    public static void RecordDbError(string operation, string type) => 
        DbErrorsTotal.Add(1, 
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("error_type", type));
}