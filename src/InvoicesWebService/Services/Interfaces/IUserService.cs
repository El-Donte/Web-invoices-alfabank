using InvoicesWebService.Models.Requests;
using InvoicesWebService.Models.Responses;
using Shared.Results;

namespace InvoicesWebService.Services.Interfaces;

public interface IUserService
{
    public Task<Result<UserResponse>> Login(UserLoginRequest request, CancellationToken ct);
    public Task<Result<Guid>> Register(UserRegisterRequest request, CancellationToken ct);
}