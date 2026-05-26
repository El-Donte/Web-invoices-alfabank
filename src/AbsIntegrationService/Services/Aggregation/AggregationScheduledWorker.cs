using System.Diagnostics;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AbsIntegrationService.Workers;

public sealed class AggregationScheduledWorker(
    IAggregationService aggregationService,
    IOptions<AggregationWorkerSettings> _settings,
    ILogger<AggregationScheduledWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _settings.Value;
    
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var processedCount = await aggregationService.ProcessPendingBatchesAsync(stoppingToken);
            
                if (processedCount == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(settings.IntervalSeconds), stoppingToken);
                    continue;
                }
                sw.Stop();
                logger.LogInformation("Aggregation cycle completed. seconds: {Count}", sw.Elapsed.TotalSeconds);
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Aggregation cycle failed");
                await Task.Delay(TimeSpan.FromSeconds(settings.IntervalSeconds), stoppingToken);
            }
        }
    }
}