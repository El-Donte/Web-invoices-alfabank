namespace AbsIntegrationService.Infrastructure.Repositories;

public interface ICounterpartyRepository
{
    Task<Guid> GetCounterpartyIdByInnAsync(string inn, CancellationToken cancellationToken = default);
    Task<List<(string Inn, Guid Id)>> GetAllCounterpartiesAsync();
}