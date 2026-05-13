using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class DraftInvoiceRepository(AppDbContext context) : IDraftInvoiceRepository
{
    public async Task<bool> ExistsByGroupIdAsync(Guid id, CancellationToken ct = default)
    {
       return await context.DraftInvoices
           .AsNoTracking()
           .AnyAsync(d => d.AggregationGroupId == id, ct);
    }

    public async Task<DraftInvoice?> GetByGroupIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await context.DraftInvoices
            .AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.AggregationGroupId == id, ct);

        return entity;
    }

    public async Task AddWithLinesAsync(DraftInvoice draft, CancellationToken ct = default)
    {
        await context.DraftInvoices.AddAsync(draft, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid id, DraftInvoiceStatus status, string? error = null,
        CancellationToken ct = default)
    { 
        await context.DraftInvoices
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.Status, status)
                .SetProperty(d => d.ValidationError, error)
                .SetProperty(d => d.UpdatedAt, DateTime.UtcNow), ct);
    }
}