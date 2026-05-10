using App.Metrics;

namespace InvoicesWebService.Metrics;

using System.Diagnostics.Metrics;


public static class InvoiceMetrics
{
    public static readonly Histogram<double> DraftCreationDuration = 
        AppMetrics.Meter.CreateHistogram<double>("invoice.draft.creation.duration", "s", "DraftInvoice creation end-to-end latency");

    public static readonly Counter<long> DraftCreationErrorsTotal = 
        AppMetrics.Meter.CreateCounter<long>("invoice.draft.creation.errors.total", "{error}", "Draft creation failures");

    public static void RecordDraftDuration(double seconds, string status) => 
        DraftCreationDuration.Record(seconds, new KeyValuePair<string, object?>("status", status));

    public static void RecordDraftError(string errorType) => 
        DraftCreationErrorsTotal.Add(1, new KeyValuePair<string, object?>("error_type", errorType));
}