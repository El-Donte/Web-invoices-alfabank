using InvoicesWebService.Extensions;
using InvoicesWebService.Models.Requests;
using InvoicesWebService.Models.Responses;
using InvoicesWebService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoicesWebService.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService service) : Controller
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] UserRegisterRequest request, CancellationToken ct)
    {
        var result = await service.Register(request, ct);
        
        return result.ToActionResult(guid => 
            Ok(Envelope.Ok(guid)));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] UserLoginRequest request, CancellationToken ct)
    {
        var result = await service.Login(request, ct);
        
        return result.ToActionResult(Response => 
            Ok(Envelope.Ok(Response)));;
    }
}