using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class DraftInvoiceRepository(IDbContextFactory<AppDbContext> ctxFactory) : IDraftInvoiceRepository
{
    public async Task<bool> Exists(Guid id, CancellationToken ct)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        try
        {
            return await context.DraftInvoices.AsNoTracking().AnyAsync(d => d.Id == id, ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task<DraftInvoiceResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        try
        {
            if (!await Exists(id, ct))
            {
                return null;
            }

            return context.DraftInvoices
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d =>
                    new DraftInvoiceResponse(d.TransactionDate, d.NdsRate, d.TotalNdsAmount,
                        d.TotalWithNds, d.TotalWithoutNds, d.Status, d.CreatedAt))
                .First();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task<List<DraftInvoiceResponse>> GetAllAsync(CancellationToken ct)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);
        try
        {
            return await context.DraftInvoices
                .AsNoTracking()
                .Select(d =>
                    new DraftInvoiceResponse(d.TransactionDate, d.NdsRate, d.TotalNdsAmount,
                        d.TotalWithNds, d.TotalWithoutNds, d.Status, d.CreatedAt))
                .ToListAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }
    
    public async Task<bool> ExistsByGroupIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            return await context.DraftInvoices
                .AsNoTracking()
                .AnyAsync(d => d.AggregationGroupId == id, ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task<DraftInvoice?> GetByGroupIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            var entity = await context.DraftInvoices
                .AsNoTracking()
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.AggregationGroupId == id, ct);

            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task AddWithLinesAsync(DraftInvoice draft, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            context.DraftInvoices.Add(draft);
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task UpdateStatusAsync(Guid id, DraftInvoiceStatus status, string? error = null,
        CancellationToken ct = default)
    { 
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        {
            await context.DraftInvoices
                .Where(d => d.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.Status, status)
                    .SetProperty(d => d.ValidationError, error)
                    .SetProperty(d => d.UpdatedAt, DateTime.UtcNow), ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }

    public async Task AddWithLinesRangeAsync(DraftInvoice[] batch, CancellationToken ct = default)
    {
        await using var context = await ctxFactory.CreateDbContextAsync(ct);

        try
        { 
            context.DraftInvoices.AddRange(batch);
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw ex;
        }
    }
}