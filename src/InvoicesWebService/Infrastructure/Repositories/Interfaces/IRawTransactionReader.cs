using InvoicesWebService.Models.DTOs;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IRawTransactionReader
{
    Task<IReadOnlyList<RawTransactionDTO>> GetByGroupIdAsync(Guid id, CancellationToken ct = default);
}