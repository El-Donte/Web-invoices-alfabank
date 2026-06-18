using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Metrics;
using InvoicesWebService.Services.Interfaces;
using Shared.Contracts.Events;
using Shared.Entities;

namespace InvoicesWebService.Services.DraftServices;

public sealed class DraftInvoiceCreationCreationService : IDraftInvoiceCreationService, IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DraftInvoiceCreationCreationService> _logger;
    
    private readonly BufferBlock<AggregationReadyEvent> _inputBuffer;
    private readonly TransformBlock<AggregationReadyEvent, DraftInvoice?> _processingBlock;
    private readonly BatchBlock<DraftInvoice> _batchBlock;
    private readonly ActionBlock<DraftInvoice[]> _saveBlock;

    public DraftInvoiceCreationCreationService(
        IServiceScopeFactory scopeFactory,
        ILogger<DraftInvoiceCreationCreationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _inputBuffer = new BufferBlock<AggregationReadyEvent>(new DataflowBlockOptions
        {
            BoundedCapacity = 50_000
        });

        _processingBlock = new TransformBlock<AggregationReadyEvent, DraftInvoice?>(
            async evt => await ProcessSingleEventAsync(evt),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 16,
                BoundedCapacity = 20_000,
                EnsureOrdered = false
            });

        _batchBlock = new BatchBlock<DraftInvoice>(1000, new GroupingDataflowBlockOptions
        {
            BoundedCapacity = 20_000
        });

        _saveBlock = new ActionBlock<DraftInvoice[]>(
            async batch => await SaveBatchAsync(batch),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 10
            });
        
        _inputBuffer.LinkTo(_processingBlock, new DataflowLinkOptions { Append = true });
        _processingBlock.LinkTo(_batchBlock, 
            new DataflowLinkOptions { Append = true },
            draft => draft != null);

        _batchBlock.LinkTo(_saveBlock, new DataflowLinkOptions { Append = true });
    }

    private async Task<DraftInvoice?> ProcessSingleEventAsync(AggregationReadyEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var txRepo = scope.ServiceProvider.GetRequiredService<IRawTransactionReader>();
            var draftRepo = scope.ServiceProvider.GetRequiredService<IDraftInvoiceRepository>();

            var transactions = await txRepo.GetByGroupIdAsync(evt.AggregationGroupId);

            if (transactions.Count == 0)
            {
                _logger.LogWarning("No transactions found for group {GroupId}", evt.AggregationGroupId);
                return null;
            }

            var draft = await draftRepo.GetByGroupIdAsync(evt.AggregationGroupId);

            if (draft == null)
            {
                draft = DraftInvoiceFactory.Create(evt, transactions);
            }
            else
            {
                DraftInvoiceFactory.Update(draft, transactions);
            }

            return draft;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AggregationReadyEvent {GroupId}", evt.AggregationGroupId);
            InvoiceMetrics.RecordDraftError(ex.GetType().Name);
            return null;
        }
    }

    private async Task SaveBatchAsync(DraftInvoice[] batch)
    {
        if (batch.Length == 0) return;
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var draftRepo = scope.ServiceProvider.GetRequiredService<IDraftInvoiceRepository>();

            await draftRepo.AddWithLinesRangeAsync(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Count} drafts", batch.Length);
            InvoiceMetrics.RecordDraftError(ex.GetType().Name);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _inputBuffer.Complete();
        await Task.WhenAll(
            _inputBuffer.Completion,
            _processingBlock.Completion,
            _batchBlock.Completion,
            _saveBlock.Completion);
    }

    public async Task ProcessAggregationReadyAsync(List<ConsumeResult<string, AggregationReadyEvent>> batch, CancellationToken ct = default)
    {
        if (batch.Count == 0)
            return;

        var sendTasks = batch.Select(item => _inputBuffer.SendAsync(item.Message.Value, ct));
        await Task.WhenAll(sendTasks);
    }
}