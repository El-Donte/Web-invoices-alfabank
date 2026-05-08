namespace AbsIntegrationService.Services.Aggregation;

public class AggregationWorkerSettings
{
    public int IntervalSeconds {get; set;}
    public int BatchSize {get; set;}
    public int MinimumTransactionsForReady {get; set;}
    public int TimeoutMinutes {get; set;}
}