using AbsIntegrationService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared;
using Shared.Configurations;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();
    public DbSet<AggregationGroup> AggregationGroups => Set<AggregationGroup>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
            {
                dataSourceBuilder.ConnectionStringBuilder.ApplicationName = "Abs-Integration-Service";
                dataSourceBuilder.ConnectionStringBuilder.SslMode = SslMode.Prefer;
                dataSourceBuilder.ConnectionStringBuilder.Multiplexing = true;
                dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 250;
            });
            
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(15);
        })
            .AddInterceptors(new AuditInterceptor())
            .AddInterceptors(new EfCoreMetricsInterceptor())
            .EnableSensitiveDataLogging(false);
    }
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasDefaultSchema("public");
        
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        b.ApplyConfigurationsFromAssembly(typeof(AggregationGroupConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(RawTransactionConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(ProcessingErrorConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(CounterpartyConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DepartmentConfiguration).Assembly);
        
        b.Ignore<User>();
        b.Ignore<DraftInvoice>();
        b.Ignore<DraftInvoiceLine>();
        b.Ignore<Invoice>();
        b.Ignore<InvoiceLine>();
        b.Ignore<ExportRecord>();
        b.Ignore<InvoiceFieldChangeHistory>();
        b.Ignore<DepartmentAccess>();
    }
}