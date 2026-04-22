using AbsIntegrationService.Models;

namespace AbsIntegrationService.Infrastructure.Repositories;

public interface IInvoiceDraftRepository
{
    Task<InvoiceDraft?> GetDraftByOperationNumberAsync(string operationNumber);
    Task<InvoiceDraft?> GetByIdAsync(Guid id);
    Task SaveDraftAsync(InvoiceDraft draft);
    Task AddLineToDraftAsync(Guid draftId, InvoiceDraftLine line);
    Task<bool> ExistsAsync(string operationNumber);
    Task MarkAsReadyAsync(Guid draftId);
}