using System.Security.Cryptography;
using System.Text;
using Shared.Contracts;
using AbsIntegrationService.Infrastructure.Repositories;
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
        if (batch.Count == 0) return new(0, 0, 0, 0);

        var validTransactions = new List<RawTransaction>();
        var validationErrorCount = 0;
        
        foreach (var (rawPayload, msg) in batch)
        {
            var validationError = validationService.Validate(msg);
            if (!string.IsNullOrEmpty(validationError))
            {
                validationErrorCount++;
                await errorService.LogAsync(new ErrorLogEntry(ProcessingStage.Ingest, "VALIDATION_FAILED", 
                    validationError, rawPayload, Retryable: false), ct);
                continue;
            }
            
            var counterpartyId = await counterpartyRepo.GetCounterpartyIdByInnAsync(msg.BuyerInn, ct);
            var hash = ComputeHash(rawPayload);
            
            validTransactions.Add(MapToRawTransaction(msg, rawPayload, hash, counterpartyId));
        }

        if (validTransactions.Count == 0) 
            return new IngestionResult(batch.Count, 0, 0, validationErrorCount);
        
        var hashes = validTransactions.Select(t => t.PayloadHash).Distinct().ToList();
        var existingHashes = await transactionRepo.GetExistingHashesAsync(hashes, ct);
        
        var duplicates = validTransactions.Count(t => existingHashes.Contains(t.PayloadHash));
        var toInsert = validTransactions.Where(t => !existingHashes.Contains(t.PayloadHash)).ToList();
        
        if(toInsert.Count > 0)
            await transactionRepo.AddRangeAsync(toInsert, ct);
        logger.LogInformation($"{toInsert.Count} inserted {validTransactions.Count} inserted transactions");
        return new IngestionResult(batch.Count, toInsert.Count, duplicates, validationErrorCount);
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