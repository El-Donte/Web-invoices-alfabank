using InvoicesWebService.Extensions;
using InvoicesWebService.Models.Responses;
using InvoicesWebService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoicesWebService.Controllers;

[ApiController]
[Route("api/draft")]
public class DraftInvoiceController(IDraftInvoiceService service) : Controller
{
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync(int limit, int offset, CancellationToken ct)
    {
        var result = await service.GetAllDrafts(ct);
        
        return result.ToActionResult(drafts => 
            Ok(Envelope.Ok(drafts)));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetDraft(id, ct);
        
        return result.ToActionResult(draft => 
            Ok(Envelope.Ok(draft)));
    }

    [HttpPost("confirm/{id:guid}")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await service.Confirm(id, ct);
        
        return result.ToActionResult(success => 
            Ok(Envelope.Ok(success)));
    }
    
    [HttpPost("decline/{id:guid}")]
    public async Task<IActionResult> Decline(Guid id, string error, CancellationToken ct)
    {
        var result = await service.Decline(id, error, ct);
        
        return result.ToActionResult(success => 
            Ok(Envelope.Ok(success)));
    }
}