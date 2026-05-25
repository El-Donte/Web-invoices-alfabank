using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AbsIntegrationService.Workers;

public sealed class AggregationScheduledWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AggregationWorkerSettings> _settings,
    ILogger<AggregationScheduledWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _settings.Value;
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.IntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var aggregationService = scope.ServiceProvider.GetRequiredService<IAggregationService>();
                
                var processedCount = await aggregationService.ProcessPendingBatchesAsync(stoppingToken);
                
                if (processedCount > 0)
                   logger.LogInformation("Aggregation cycle completed. Groups updated: {Count}", processedCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Aggregation cycle failed. Will retry in {Interval}s", settings.IntervalSeconds);
            }
        }
    }
}