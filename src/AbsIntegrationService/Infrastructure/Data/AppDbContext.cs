using AbsIntegrationService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext
{
    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();
    public DbSet<AggregationGroup> AggregationGroups => Set<AggregationGroup>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();
    public DbSet<Counterparty> Counterparties => Set<Counterparty>(); 
    public DbSet<Department> Departments => Set<Department>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
            {
                dataSourceBuilder.ConnectionStringBuilder.ApplicationName = "Abs-Integration-Service";
                dataSourceBuilder.ConnectionStringBuilder.SslMode = SslMode.Require;
                dataSourceBuilder.ConnectionStringBuilder.Multiplexing = true;
                dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 150;
                dataSourceBuilder.ConnectionStringBuilder.KeepAlive = 30;
            });
            
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(15);
        });
    }
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasDefaultSchema("public");
        
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        b.ApplyConfigurationsFromAssembly(typeof(AggregationGroupConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(CounterpartyConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DepartmentConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(RawTransactionConfiguration).Assembly);
    }
}