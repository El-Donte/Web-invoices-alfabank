using InvoicesWebService.Models.Responses;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories.Interfaces;

public interface IDraftInvoiceRepository
{
    Task<bool> ExistsByGroupIdAsync(Guid id, CancellationToken ct = default);
    Task<DraftInvoice?> GetByGroupIdAsync(Guid id, CancellationToken ct = default);

    Task AddWithLinesAsync(DraftInvoice draft, CancellationToken ct = default);

    Task UpdateStatusAsync(Guid id, DraftInvoiceStatus status, string? error = null, CancellationToken ct = default);
    
    Task AddWithLinesRangeAsync(DraftInvoice[] batch,  CancellationToken ct = default);

    Task<bool> Exists(Guid id, CancellationToken ct);
    Task<DraftInvoiceResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DraftInvoiceResponse>> GetAllAsync(CancellationToken ct);
}