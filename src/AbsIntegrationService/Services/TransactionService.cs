using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Shared.Contracts;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Metrics;
using AbsIntegrationService.Services.Interfaces;
using Shared;
using Shared.Entities;

namespace AbsIntegrationService.Services;

public sealed class TransactionIngestionService : ITransactionIngestionService, IAsyncDisposable
{
    private readonly  IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionIngestionService> _logger;
    
    private readonly BufferBlock<(string RawPayload, AbsMessage Message)> _inputBuffer;
    private readonly TransformBlock<(string RawPayload, AbsMessage Message), RawTransaction?> _processingBlock;
    private readonly BatchBlock<RawTransaction> _batchBlock;
    private readonly ActionBlock<RawTransaction[]> _saveBlock;
    
    private readonly ConcurrentDictionary<string, Guid> _counterpartyCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private bool _cacheInitialized;
    
    public TransactionIngestionService(
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionIngestionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _inputBuffer = new BufferBlock<(string, AbsMessage)>(new DataflowBlockOptions
        {
            BoundedCapacity = 100_000
        });

        _processingBlock = new TransformBlock<(string RawPayload, AbsMessage Message), RawTransaction?>(
            async item => await ProcessSingleItemAsync(item),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 8,
                BoundedCapacity = 50_000,
                EnsureOrdered = false
            });

        _batchBlock = new BatchBlock<RawTransaction>(10, new GroupingDataflowBlockOptions
        {
            BoundedCapacity = 50_000
        });

        _saveBlock = new ActionBlock<RawTransaction[]>(
            async batch => await SaveBatchAsync(batch),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 10
            });
        
        _inputBuffer.LinkTo(_processingBlock, new DataflowLinkOptions { Append = true });

        _processingBlock.LinkTo(_batchBlock, 
            new DataflowLinkOptions { Append = true },
            item => item != null);

        _batchBlock.LinkTo(_saveBlock, new DataflowLinkOptions { Append = true });
    }
    
    private async Task EnsureCacheInitializedAsync(CancellationToken ct = default)
    {
        if (_cacheInitialized) return;
    
        await _cacheLock.WaitAsync(ct);
        
        try
        {
            if (_cacheInitialized) return;
        
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICounterpartyRepository>();
            var allCounterparties = await repo.GetAllCounterpartiesAsync();
        
            foreach (var cp in allCounterparties
                         .Where(cp => !string.IsNullOrWhiteSpace(cp.Inn)))
            {
                _counterpartyCache.TryAdd(cp.Inn, cp.Id);
            }
        
            _cacheInitialized = true;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<RawTransaction?> ProcessSingleItemAsync(
        (string RawPayload, AbsMessage Message) item)
    {
        var (rawPayload, msg) = item;
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var validationService = scope.ServiceProvider.GetRequiredService<IValidationService>();
            var errorService = scope.ServiceProvider.GetRequiredService<IProcessingErrorService>();
            
            var validationError = validationService.Validate(msg);
            if (validationError != null)
            {
                IngestionMetrics.RecordMessage("validation_error");
                _ = errorService.LogAsync(new ErrorLogEntry(ProcessingStage.Ingest,
                    "VALIDATION_FAILED", validationError, rawPayload, false));
                return null;
            }

            await EnsureCacheInitializedAsync();
        
            if (!_counterpartyCache.TryGetValue(msg.BuyerInn, out var counterpartyId))
            {
                _logger.LogWarning("Counterparty not found for INN: {Inn}", msg.BuyerInn);
                return null;
            }

            var hash = ComputeHash(rawPayload);
            return MapToRawTransaction(msg, rawPayload, hash, counterpartyId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process item {OperationNumber}", msg?.OperationNumber);
            return null;
        }
    }

    private async Task SaveBatchAsync(IReadOnlyList<RawTransaction> batch)
    {
        if (batch.Count == 0) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRawTransactionRepository>();
            
            await repo.AddRangeAsync(batch);
            
            IngestionMetrics.RecordMessage("db_save_success", batch.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Save cancelled for batch of {Count}", batch.Count);
            throw;
        }
        catch (Exception ex)
        {
            IngestionMetrics.RecordMessage("db_save_error", batch.Count);
            _logger.LogError(ex, "Failed to save batch of {Count}", batch.Count);
        }
    }

    public async Task<IngestionResult> ProcessBatchAsync(
        IReadOnlyList<(string RawPayload, AbsMessage Message)> batch,
        CancellationToken ct = default)
    {
        if (batch.Count == 0)
            return new IngestionResult(0, 0, 0);

        var sendTasks = batch.Select(item => _inputBuffer.SendAsync(item, ct));
        await Task.WhenAll(sendTasks);

        return new IngestionResult(batch.Count, 0, 0);
    }

    public async ValueTask DisposeAsync()
    {
        _inputBuffer.Complete();

        await Task.WhenAll(
            _inputBuffer.Completion,
            _processingBlock.Completion,
            _batchBlock.Completion,
            _saveBlock.Completion
        );
    
        _cacheLock?.Dispose();
    }
    
     private static RawTransaction MapToRawTransaction(AbsMessage message, string payload, string hash, Guid counterpartyId) =>
         new()
         {
             Id = Guid.NewGuid(),
             OperationNumber = message.OperationNumber,
             Type = message.Type,
             Date = message.OperationDate,
             ProductCode = message.ProductCode,
             ProductName = message.ProductName,
             UnitMeasure = message.Unit,
             CurrencyCode = message.CurrencyCode,
             Quantity = message.Quantity,
             UnitPrice = message.UnitPrice,
             NdsRate = message.NdsRate,
             NdsAmount = message.NdsAmount,
             Amount = message.Amount,
             PayloadHash = hash,
             Payload = payload,
             CounterpartyId = counterpartyId,
             DepartmentId = message.DepartmentId,
             Status = TransactionStatus.Processed, 
             ReceivedAt = DateTime.UtcNow
         };
 
     private static string ComputeHash(string input) => 
         Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}