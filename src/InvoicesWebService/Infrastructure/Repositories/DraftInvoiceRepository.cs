// using InvoicesWebService.Infrastructure.Data;
// using InvoicesWebService.Infrastructure.Repositories.Interfaces;
// using Microsoft.EntityFrameworkCore;
// using Shared.Entities;
//
// namespace InvoicesWebService.Infrastructure.Repositories;
//
// public class DraftInvoiceRepository(AppDbContext context) : IDraftInvoiceRepository
// {
//     public async Task<DraftInvoice?> GetDraftByOperationNumberAsync(string operationNumber,CancellationToken ct)
//     {
//        var entity = await context.DraftInvoices
//             .AsNoTracking()
//             .Include(d => d.Lines)
//             .FirstOrDefaultAsync(d => d.OperationNumber == operationNumber, ct);
//        
//        return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
//     }
//
//     public async Task<DraftInvoice?> GetByIdAsync(Guid id, CancellationToken ct)
//     {
//         var entity = await context.DraftInvoices
//             .AsNoTracking()
//             .Include(d => d.Lines)
//             .Include(d => d.LinkedOperations)
//             .FirstOrDefaultAsync(d => d.Id == id, ct);
//         
//         return entity == null ? null : mapper.Map<InvoiceDraft>(entity);
//     }
//
//     public async Task AddDraftAsync(DraftInvoice draft, CancellationToken ct)
//     {
//         var entity = mapper.Map<InvoiceDraftEntity>(draft);
//         
//         context.DraftInvoices.Add(entity);
//
//         await context.SaveChangesAsync(ct);
//     }
//
//     public async Task UpdateDraftAsync(DraftInvoice draft, CancellationToken ct)
//     {
//         InvoiceDraftEntity? entity = await context.InvoicesDrafts.FirstOrDefaultAsync(i => i.Id == draft.Id, ct);
//         
//         if (entity == null)
//         {
//             return;
//         }
//         
//         context.DraftInvoices.Update(mapper.Map(draft, entity));
//         
//         await context.SaveChangesAsync(ct);
//     }
//
//     public async Task AddDraftLineAsync(Guid draftId, DraftInvoiceLine line, CancellationToken ct)
//     {
//         var draft = await GetByIdAsync(draftId, ct);
//         
//         if (draft == null)
//             throw new InvalidOperationException($"Draft with id {draftId} not found");
//
//         context.InvoiceLines.Add();
//
//         await context.SaveChangesAsync(ct);
//     }
//
//     public async Task<bool> ExistsAsync(string operationNumber,CancellationToken ct)
//     {
//         return await context.DraftInvoices
//             .AnyAsync(d => d.OperationNumber == operationNumber, ct);
//     }
//
//     public async Task MarkAsReadyAsync(Guid draftId,CancellationToken ct)
//     {
//         var draft = await context.DraftInvoices.FindAsync(draftId, ct);
//         
//         if (draft != null)
//         {
//             draft.Status = DraftStatus.Ready;
//             draft.ProcessedAt = DateTime.UtcNow;
//             await context.SaveChangesAsync(ct);
//         }
//     }
// }