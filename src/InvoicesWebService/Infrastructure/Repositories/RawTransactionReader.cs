using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class RawTransactionReader(AppDbContext context) : IRawTransactionReader
{
    public async Task<IReadOnlyList<RawTransactionDTO>> GetByGroupIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<RawTransaction>()
            .AsNoTracking()
            .Where(t => t.AggregationGroupId == id && t.Status == TransactionStatus.Processed)
            .Select(t => new RawTransactionDTO(
                t.Id, t.ProductName, t.ProductCode, t.UnitMeasure, t.Quantity, t.UnitPrice, t.Amount, t.NdsAmount,
                t.CounterpartyId.Value, t.DepartmentId.Value, t.CurrencyCode, t.NdsRate))
            .ToListAsync(ct);
    }
}