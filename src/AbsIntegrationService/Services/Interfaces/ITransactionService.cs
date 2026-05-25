using Shared.Contracts;

namespace AbsIntegrationService.Services.Interfaces;

public interface ITransactionIngestionService
{
    Task<IngestionResult> ProcessBatchAsync(
        IReadOnlyList<(string RawPayload, AbsMessage Message)> batch, 
        CancellationToken ct = default);
}

public record IngestionResult(int Total, int Duplicates, int ValidationErrors);