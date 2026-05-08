using System.Text.Json;
using Confluent.Kafka;
using InvoicesWebService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.Kafka;

public class AggregationReadyConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AggregationReadyConsumer> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("abs.aggregation_ready");

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
            if (result is null) continue;

            try
            {
                var evt = JsonSerializer.Deserialize<AggregationReadyEvent>(result.Message.Value);
                await ProcessAsync(evt, stoppingToken);
                _consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process AggregationReadyEvent. Offset NOT committed.");
            }
        }
    }

    private async Task ProcessAsync(AggregationReadyEvent evt, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        if (await db.DraftInvoices.AnyAsync(d => d.AggregationGroupId == evt.AggregationGroupId, ct))
            return;
        
        var transactions = await db.RawTransactions
            .AsNoTracking()
            .Where(t => t.AggregationGroupId == evt.AggregationGroupId && t.Status == TransactionStatus.Processed)
            .ToListAsync(ct);
        
        // var draft = DraftInvoiceFactory.Create(evt.AggregationGroupId, transactions);
        //
        // db.DraftInvoices.Add(draft);
        // db.AggregationGroups
        //     .Where(g => g.Id == evt.AggregationGroupId)
        //     .ExecuteUpdateAsync(s => s.SetProperty(g => g.Status, AggregationStatus.Draft), ct);
        
        await db.SaveChangesAsync(ct);
    }
}