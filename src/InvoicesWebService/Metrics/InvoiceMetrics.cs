using System.Diagnostics.Metrics;

namespace InvoicesWebService.Metrics;

public static class InvoiceMetrics
{
    private static readonly Meter Meter = new("InvoiceWebService", "1.0.0");

    public static readonly Histogram<double> DraftCreationDuration = 
        Meter.CreateHistogram<double>("invoice.draft.creation.duration", "s", "DraftInvoice creation end-to-end latency");

    public static readonly Counter<long> DraftCreationErrorsTotal = 
        Meter.CreateCounter<long>("invoice.draft.creation.errors.total", "{error}", "Draft creation failures");

    public static void RecordDraftDuration(double seconds, string status) => 
        DraftCreationDuration.Record(seconds, new KeyValuePair<string, object?>("status", status));

    public static void RecordDraftError(string errorType) => 
        DraftCreationErrorsTotal.Add(1, new KeyValuePair<string, object?>("error_type", errorType));
    
}