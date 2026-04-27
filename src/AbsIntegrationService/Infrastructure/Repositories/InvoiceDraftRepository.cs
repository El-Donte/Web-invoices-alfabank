using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.Entities;
using AbsIntegrationService.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class InvoiceDraftRepository(AppDbContext context,IMapper mapper) : IInvoiceDraftRepository
{
    public async Task<InvoiceDraft?> GetDraftByOperationNumberAsync(string operationNumber,CancellationToken ct)
    {
       var entity = await context.InvoicesDrafts
            .AsNoTracking()
            .Include(d => d.Lines)
            .Include(d => d.LinkedOperations)
            .FirstOrDefaultAsync(d => d.OperationNumber == operationNumber, ct);
       
       return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
    }

    public async Task<InvoiceDraft?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await context.InvoicesDrafts
            .AsNoTracking()
            .Include(d => d.Lines)
            .Include(d => d.LinkedOperations)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        
        return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
    }

    public async Task AddDraftAsync(InvoiceDraft draft, CancellationToken ct)
    {
        var entity = mapper.Map<InvoiceDraftEntity>(draft);
        
        await context.InvoicesDrafts.AddAsync(entity, ct);

        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateDraftAsync(InvoiceDraft draft, CancellationToken ct)
    {
        InvoiceDraftEntity? entity = await context.InvoicesDrafts.FirstOrDefaultAsync(i => i.Id == draft.Id, ct);
        
        if (entity == null)
        {
            return;
        }

        mapper.Map(draft, entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task AddDraftLineAsync(Guid draftId, InvoiceDraftLine line, CancellationToken ct)
    {
        var draft = await GetByIdAsync(draftId, ct);
        
        if (draft == null)
            throw new InvalidOperationException($"Draft with id {draftId} not found");

        await context.InvoiceLines.AddAsync(mapper.Map<InvoiceDraftLineEntity>(line), ct);

        await context.SaveChangesAsync(ct);
    }
    
    public async Task AddOperationLinkAsync(Guid draftId, DraftOperationLink link, CancellationToken ct)
    {
        var draft = await GetByIdAsync(draftId, ct);
        
        if (draft == null)
            throw new InvalidOperationException($"Draft with id {draftId} not found");
        
        await context.OperationLinks.AddAsync(mapper.Map<DraftOperationLinkEntity>(link), ct);

        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(string operationNumber,CancellationToken ct)
    {
        return await context.InvoicesDrafts
            .AnyAsync(d => d.OperationNumber == operationNumber, ct);
    }

    public async Task MarkAsReadyAsync(Guid draftId,CancellationToken ct)
    {
        var draft = await context.InvoicesDrafts.FindAsync(draftId, ct);
        if (draft != null)
        {
            draft.Status = DraftStatus.Ready;
            draft.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }
}