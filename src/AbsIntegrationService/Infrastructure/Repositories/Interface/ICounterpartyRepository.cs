namespace AbsIntegrationService.Infrastructure.Repositories;

public interface ICounterpartyRepository
{
    Task<Guid> GetCounterpartyIdByInnAsync(string inn, CancellationToken cancellationToken = default);
    Task<Dictionary<string, Guid>> GetCounterpartyIdsByInnBatchAsync(List<string> inns, CancellationToken ct = default);
}