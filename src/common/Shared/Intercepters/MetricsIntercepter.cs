using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Metrics;

namespace Shared;

public sealed class EfCoreMetricsInterceptor : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var baseResult = await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        sw.Stop();
        AppMetrics.RecordDbOperation("Query", sw.Elapsed.TotalSeconds);
        return baseResult;
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var baseResult = await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        sw.Stop();
        AppMetrics.RecordDbOperation("SaveChanges", sw.Elapsed.TotalSeconds);
        return baseResult;
    }
}