using Shared.Entities;

namespace InvoicesWebService.Models.Requests;

public record UserRegisterRequest(string Login, string Name, string Surname, string Password, Position Position);