using InvoicesWebService.Models.Responses;
using Shared.Results;

namespace InvoicesWebService.Services.Interfaces;

public interface IDraftInvoiceService
{
    public Task<Result<List<DraftInvoiceResponse>>> GetAllDrafts(CancellationToken ct);
    public Task<Result<DraftInvoiceResponse>> GetDraft(Guid id, CancellationToken ct);
    public Task<Result<bool>> Confirm(Guid id, CancellationToken ct);
    public Task<Result<bool>> Decline(Guid id, string error, CancellationToken ct);
}