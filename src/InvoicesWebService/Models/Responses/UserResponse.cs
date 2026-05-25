using Shared.Entities;

namespace InvoicesWebService.Models.Responses;

public record UserResponse(string accessToken, User user);