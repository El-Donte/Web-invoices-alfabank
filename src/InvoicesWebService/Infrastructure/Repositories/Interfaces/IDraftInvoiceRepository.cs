using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IDraftInvoiceRepository
{
    Task<bool> ExistsByGroupIdAsync(Guid id, CancellationToken ct = default);
    Task<DraftInvoice?> GetByGroupIdAsync(Guid id, CancellationToken ct = default);

    Task AddWithLinesAsync(DraftInvoice draft, CancellationToken ct = default);

    Task UpdateStatusAsync(Guid id, DraftInvoiceStatus status, string? error = null, CancellationToken ct = default);
    
    Task AddWithLinesRangeAsync(DraftInvoice[] batch,  CancellationToken ct = default);
}