namespace AbsIntegrationService.Services.Aggregation;

public class AggregationWorkerSettings
{
    public double IntervalSeconds {get; set;}
    public int BatchSize {get; set;}
    public int MinimumTransactionsForReady {get; set;}
    public double TimeoutMinutes {get; set;}
    public int EventBatchSize {get; set;}
    public int EventFlushIntervalMs { get; set; }
}