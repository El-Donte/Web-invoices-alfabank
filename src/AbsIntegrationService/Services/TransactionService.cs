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

            var hash = ComputeHash(rawPayload);
            validTransactions.Add(MapToRawTransaction(msg, rawPayload, hash));
        }

        if (validTransactions.Count == 0) 
            return new IngestionResult(batch.Count, 0, 0, validationErrorCount);
        
        var hashes = validTransactions.Select(t => t.PayloadHash).Distinct().ToList();
        var existingHashes = await transactionRepo.GetExistingHashesAsync(hashes, ct);
        
        var duplicates = validTransactions.Count(t => existingHashes.Contains(t.PayloadHash));
        var toInsert = validTransactions.Where(t => !existingHashes.Contains(t.PayloadHash)).ToList();
        
        if(toInsert.Count > 0)
            await transactionRepo.AddRangeAsync(toInsert, ct);
        
        return new IngestionResult(batch.Count, toInsert.Count, duplicates, validationErrorCount);
    }

    private static RawTransaction MapToRawTransaction(AbsMessage message, string payload, string hash) =>
        new()
        {
            Id = Guid.NewGuid(),
            OperationNumber = message.OperationNumber,
            Type = message.Type,
            Date = message.OperationDate,
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
            Status = TransactionStatus.Received,
            ReceivedAt = DateTime.UtcNow,
        };
    
    private static string ComputeHash(string input) => 
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}