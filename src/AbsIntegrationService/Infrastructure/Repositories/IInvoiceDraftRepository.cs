using AbsIntegrationService.Models;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IInvoiceDraftRepository
{
    Task<InvoiceDraft?> GetDraftByOperationNumberAsync(string operationNumber, CancellationToken ct);
    Task<InvoiceDraft?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddDraftAsync(InvoiceDraft draft, CancellationToken ct);
    Task UpdateDraftAsync(InvoiceDraft draft, CancellationToken ct);
    Task AddDraftLineAsync(Guid draftId, InvoiceDraftLine line, CancellationToken ct);
    Task AddOperationLinkAsync(Guid draftId, DraftOperationLink link, CancellationToken ct);
    Task<bool> ExistsAsync(string operationNumber, CancellationToken ct);
    Task MarkAsReadyAsync(Guid draftId, CancellationToken ct);
}