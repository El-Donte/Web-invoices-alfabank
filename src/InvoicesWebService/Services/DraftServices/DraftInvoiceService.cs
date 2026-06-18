using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Models.Responses;
using InvoicesWebService.Services.Interfaces;
using Shared.Entities;
using Shared.Results;

namespace InvoicesWebService.Services.DraftServices;

public class DraftInvoiceService(IDraftInvoiceRepository repository) : IDraftInvoiceService
{
    public async Task<Result<List<DraftInvoiceResponse>>>GetAllDrafts(CancellationToken ct)
    {
        return await repository.GetAllAsync(ct);
    }

    public async Task<Result<DraftInvoiceResponse>> GetDraft(Guid id, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity == null)
        {
            return Errors.DraftInvoiceErrors.MissingDraftInvoice();
        }

        return entity;
    }

    public async Task<Result<bool>> Confirm(Guid id, CancellationToken ct)
    {
        if (! await repository.Exists(id, ct))
        {
            return Errors.DraftInvoiceErrors.MissingDraftInvoice();
        }
        
        await repository.UpdateStatusAsync(id, DraftInvoiceStatus.Confirmed, ct: ct);
        return true;
    }

    public async Task<Result<bool>> Decline(Guid id, string error, CancellationToken ct)
    {
        if (!await repository.Exists(id, ct))
        {
            return Errors.DraftInvoiceErrors.MissingDraftInvoice();
        }
        
        await repository.UpdateStatusAsync(id, DraftInvoiceStatus.ValidatingError, error, ct);
        return true;
    }
}