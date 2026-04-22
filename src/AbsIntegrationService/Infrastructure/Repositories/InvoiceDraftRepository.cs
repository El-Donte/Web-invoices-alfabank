using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.Entities;
using AbsIntegrationService.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Repositories;

public class InvoiceDraftRepository(AppDbContext context,IMapper mapper) : IInvoiceDraftRepository
{
    public async Task<InvoiceDraft?> GetDraftByOperationNumberAsync(string operationNumber)
    {
       var entity = await context.InvoicesDrafts
            .AsNoTracking()
            .Include(d => d.Lines)
            .Include(d => d.LinkedOperations)
            .FirstOrDefaultAsync(d => d.OperationNumber == operationNumber);
       
       return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
    }

    public async Task<InvoiceDraft?> GetByIdAsync(Guid id)
    {
        var entity = await context.InvoicesDrafts
            .AsNoTracking()
            .Include(d => d.Lines)
            .Include(d => d.LinkedOperations)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
    }

    public async Task SaveDraftAsync(InvoiceDraft draft)
    {
        if (draft.Id == Guid.Empty)
        {
            draft.Id = Guid.NewGuid();
            context.InvoicesDrafts.Add(mapper.Map<InvoiceDraftEntity>(draft));
        }
        else
        {
            context.InvoicesDrafts.Update(mapper.Map<InvoiceDraftEntity>(draft));
        }

        await context.SaveChangesAsync();
    }

    public async Task AddLineToDraftAsync(Guid draftId, InvoiceDraftLine line)
    {
        var draft = await GetByIdAsync(draftId);
        
        if (draft == null)
            throw new InvalidOperationException($"Draft with id {draftId} not found");

        line.InvoiceDraftId = draftId;
        draft.Lines.Add(line);
        
        draft.LinkedOperations.Add(new DraftOperationLink
        {
            InvoiceDraftId = draftId,
            OperationNumber = draft.OperationNumber,
            OperationDate = draft.OperationDate,
            Amount = line.AmountWithNds,
            SourceMessageId = Guid.NewGuid().ToString()
        });

        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string operationNumber)
    {
        return await context.InvoicesDrafts
            .AnyAsync(d => d.OperationNumber == operationNumber);
    }

    public async Task MarkAsReadyAsync(Guid draftId)
    {
        var draft = await context.InvoicesDrafts.FindAsync(draftId);
        if (draft != null)
        {
            draft.Status = DraftStatus.Ready;
            draft.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}