namespace AbsIntegrationService.Services.Interfaces;

public interface IErrorHandlingService
{
    Task LogErrorAsync(string code, string message, string payload, bool retryable, CancellationToken ct);
}