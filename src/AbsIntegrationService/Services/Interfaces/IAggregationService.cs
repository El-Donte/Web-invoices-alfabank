namespace AbsIntegrationService.Services.Interfaces;

public interface IAggregationService
{
    Task<int> ProcessPendingBatchesAsync(CancellationToken ct = default);
}