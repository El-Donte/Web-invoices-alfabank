using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IRawTransactionReader
{
    Task<IReadOnlyList<RawTransaction>> GetByGroupIdAsync(Guid id, CancellationToken ct = default);
}