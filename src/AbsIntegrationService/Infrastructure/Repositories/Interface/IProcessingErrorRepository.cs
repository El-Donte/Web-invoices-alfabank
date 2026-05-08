using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IProcessingErrorRepository
{
    Task LogAsync(ProcessingError error, CancellationToken ct = default);

    Task<IReadOnlyList<ProcessingError>> GetRetryableAsync(ProcessingStage stage, int limit,
        CancellationToken ct = default);

    Task MarkResolvedAsync(Guid errorId, CancellationToken ct = default);

}