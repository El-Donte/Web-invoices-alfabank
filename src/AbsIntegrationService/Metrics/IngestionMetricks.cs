using App.Metrics;

namespace AbsIntegrationService.Metrics;

using System.Diagnostics.Metrics;

public static class IngestionMetrics
{
    public static readonly Counter<long> ValidationErrorsTotal = 
        AppMetrics.Meter.CreateCounter<long>("ingestion.validation.errors.total", "{error}", "Validation failures during ingestion");

    public static readonly Counter<long> DuplicateMessagesTotal = 
        AppMetrics.Meter.CreateCounter<long>("ingestion.duplicate.messages.total", "{message}", "Idempotent duplicate drops");

    public static void RecordValidation(string errorType) => 
        ValidationErrorsTotal.Add(1, new KeyValuePair<string, object?>("error_type", errorType));

    public static void RecordDuplicate() => 
        DuplicateMessagesTotal.Add(1);
}