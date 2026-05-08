using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IDraftInvoiceRepository
{
    Task<DraftInvoice?> GetDraftByOperationNumberAsync(string operationNumber, CancellationToken ct);
    Task<DraftInvoice?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddDraftAsync(DraftInvoice draft, CancellationToken ct);
    Task UpdateDraftAsync(DraftInvoice draft, CancellationToken ct);
    Task AddDraftLineAsync(Guid draftId, DraftInvoiceLine line, CancellationToken ct);
    Task<bool> ExistsAsync(string operationNumber, CancellationToken ct);
    Task MarkAsReadyAsync(Guid draftId, CancellationToken ct);
}