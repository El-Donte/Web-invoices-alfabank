using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services.Interfaces;
using Shared.Entities;

namespace AbsIntegrationService.Services;

public class ErrorHandlingService(IProcessingErrorRepository errorRepo) : IErrorHandlingService
{
    public async Task LogErrorAsync(string code, string message, string payload, bool retryable, CancellationToken ct)
    {
        await errorRepo.LogAsync(new ProcessingError
        {
            Stage = ProcessingStage.Ingest,
            Code = code,
            Message = message,
            Payload = payload,
            Retryable = retryable,
            RawTransactionId = Guid.Empty
        }, ct);
    }
}