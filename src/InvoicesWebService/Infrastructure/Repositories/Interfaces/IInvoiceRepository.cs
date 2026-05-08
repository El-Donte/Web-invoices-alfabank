using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Invoice?> GetByDraftId(Guid draftId, CancellationToken ct);
    Task<Invoice?> GetByNumberAsync(string number, CancellationToken ct);
    Task<List<Invoice>> GetAllAsync(CancellationToken ct);
    Task CreateAsync(Invoice invoice, CancellationToken ct);
    Task UpdateAsync(Invoice invoice, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task AddLineAsync(Guid invoiceId, InvoiceLine invoiceLine, CancellationToken ct);
    Task<bool> ExistAsync(Invoice invoice, CancellationToken ct);
}