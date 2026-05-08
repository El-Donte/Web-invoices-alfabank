using Shared.Contracts;

namespace AbsIntegrationService.Services.Interfaces;

public interface IValidationService
{
    string Validate(AbsMessage msg);
}