using InvoicesWebService.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Npgsql;
using Shared;

namespace InvoicesWebService.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options, IConfiguration configuration)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentAccess> DepartmentAccesses => Set<DepartmentAccess>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
                {
                    dataSourceBuilder.ConnectionStringBuilder.ApplicationName = "Invoices-Web-Service";
                    dataSourceBuilder.ConnectionStringBuilder.SslMode = SslMode.Prefer;
                    dataSourceBuilder.ConnectionStringBuilder.Multiplexing = true;
                    dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 150;
                });
            
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 2,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            
            })
            .AddInterceptors(new AuditInterceptor())
            .AddInterceptors(new EfCoreMetricsInterceptor())
            .EnableSensitiveDataLogging(false);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        b.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DepartmentAccessConfiguration).Assembly);

        b.Ignore<Department>();
        b.Ignore<DraftInvoice>();
        b.Ignore<DraftInvoiceLine>();
        b.Ignore<Invoice>();
        b.Ignore<InvoiceLine>();
        b.Ignore<InvoiceFieldChangeHistory>();
        b.Ignore<Counterparty>();
        b.Ignore<AggregationGroup>();
        b.Ignore<RawTransaction>();
        b.Ignore<ExportRecord>();
    }
}
