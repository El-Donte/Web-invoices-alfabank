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

public sealed class TransactionIngestionService(
    IRawTransactionRepository transactionRepo,
    IProcessingErrorService errorService,
    IValidationService validationService,
    ICounterpartyRepository counterpartyRepo,
    ILogger<TransactionIngestionService> logger)
    : ITransactionIngestionService
{

    public async Task<IngestionResult> ProcessBatchAsync(
        IReadOnlyList<(string RawPayload, AbsMessage Message)> batch, 
        CancellationToken ct = default)
    {
        if (batch.Count == 0) return new IngestionResult(0, 0, 0, 0);

        var validTransactions = new ConcurrentBag<RawTransaction>();
        var validationErrorCount = 0;

        try
        {
            var innToProcess = new ConcurrentDictionary<string, byte>();
            await Parallel.ForEachAsync(batch, new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = 16
            }, async (item, token) =>
            {
                var (rawPayload, msg) = item;

                var validationError = validationService.Validate(msg);
                if (!string.IsNullOrEmpty(validationError))
                {
                    Interlocked.Increment(ref validationErrorCount);
                    IngestionMetrics.RecordMessage("validation_error");

                    await errorService.LogAsync(new ErrorLogEntry(
                        ProcessingStage.Ingest,
                        "VALIDATION_FAILED",
                        validationError, rawPayload, false), token);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(msg.BuyerInn))
                    innToProcess.TryAdd(msg.BuyerInn, 0);
            });
            
            var innList = innToProcess.Keys.ToList();
            var counterpartyDict = innList.Count > 0 
                ? await counterpartyRepo.GetCounterpartyIdsByInnBatchAsync(innList, ct) 
                : new Dictionary<string, Guid>();
            
            await Parallel.ForEachAsync(batch, new ParallelOptions 
            { 
                MaxDegreeOfParallelism = 24, 
                CancellationToken = ct 
            }, (item, token) =>
            {
                var (rawPayload, msg) = item;

                if (string.IsNullOrWhiteSpace(msg.BuyerInn) || 
                    !counterpartyDict.TryGetValue(msg.BuyerInn, out var counterpartyId))
                {
                    return ValueTask.CompletedTask;
                }

                var hash = ComputeHash(rawPayload);
                var transaction = MapToRawTransaction(msg, rawPayload, hash, counterpartyId);
                validTransactions.Add(transaction);

                return ValueTask.CompletedTask;
            });
            

            if (validTransactions.Count == 0)
                return new IngestionResult(batch.Count, 0, 0, validationErrorCount);

            var validList = validTransactions.ToList();

            var hashes = validList.Select(t => t.PayloadHash).Distinct().ToList();
            var existingHashes = await transactionRepo.GetExistingHashesAsync(hashes, ct);

            var toInsert = validList
                .Where(t => !existingHashes.Contains(t.PayloadHash))
                .ToList();

            var duplicates = validList.Count - toInsert.Count;

            if (toInsert.Count > 0)
                await transactionRepo.AddRangeAsync(toInsert, ct);

            logger.LogInformation("Batch processed: {Total} total, {Inserted} inserted, {Duplicates} duplicates",
                batch.Count, toInsert.Count, duplicates);

            return new IngestionResult(batch.Count, toInsert.Count, duplicates, validationErrorCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing batch");
        }
        
        return new IngestionResult(batch.Count, 0, 0, 0);
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