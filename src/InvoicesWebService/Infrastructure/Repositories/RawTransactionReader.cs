using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Repositories;

public class RawTransactionReader(AppDbContext context) : IRawTransactionReader
{
    public async Task<IReadOnlyList<RawTransaction>> GetByGroupIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Set<RawTransaction>()
            .AsNoTracking()
            .Where(t => t.AggregationGroupId == id && t.Status == TransactionStatus.Processed)
            .ToListAsync(ct);
    }

    // public async Task<IReadOnlyList<TransactionSummary> GetSummaryByGroupIdAsync(Guid id,
    //     CancellationToken ct = default)
    // {
    //     await context.RawTransactions
    //         .AsNoTracking()
    //         .Where(t => t.AggregationGroupId == id)
    //         .Select(t => new RawTransactionSummary(
    //             t.Id, t.ProductName, t.UnitMeasure, t.Quantity, t.UnitPrice, t.Amount, t.NdsAmount))
    //         .ToListAsync(ct);
    // }
}