using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shared;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntity.CreatedAt = now;
                        auditEntity.UpdatedAt = now;
                        break;
                        
                    case EntityState.Modified:
                        auditEntity.UpdatedAt = now;

                        context.Entry(auditEntity).Property(e => e.CreatedAt).IsModified = false;
                        break;
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}