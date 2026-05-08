using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class InvoiceRepository(AppDbContext context) : IInvoiceRepository
{
    public async Task<Invoice> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await context.Invoices
            .AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        return entity;
    }

    public async Task<Invoice?> GetByDraftId(Guid draftId, CancellationToken ct)
    {
        var entity = await context.Invoices
            .AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.DraftInvoiceId == draftId, ct);

        return entity;
    }

    public async Task<Invoice?> GetByNumberAsync(string number, CancellationToken ct)
    {
        var entity = await context.Invoices
            .AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Number == number, ct);
        
        return entity;
    }

    public async Task AddLineAsync(Guid invoiceId, InvoiceLine invoiceLine, CancellationToken ct)
    {
        var invoice = await GetByIdAsync(invoiceId,  ct);

        if (invoice == null)
        {
            return;
        }

        var lineEntity = new InvoiceLine();
        
        context.InvoiceLines.Add(lineEntity);
        
        await context.SaveChangesAsync(ct);
    }

    public Task<List<Invoice>> GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistAsync(Invoice invoice, CancellationToken ct)
    {
       return await context.Invoices
            .AnyAsync(x => x.Number == invoice.Number, ct);
    }
    
    public async Task CreateAsync(Invoice invoice, CancellationToken ct)
    {
        var exists = await ExistAsync(invoice, ct);

        if (exists) return;

        var entity = new Invoice();
        
        context.Invoices.Add(entity);

        await context.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}