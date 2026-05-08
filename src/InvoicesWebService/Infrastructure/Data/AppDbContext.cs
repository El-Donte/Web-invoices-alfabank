using InvoicesWebService.Infrastructure.Configurations;
using Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InvoicesWebService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext
{
    public DbSet<DraftInvoice> DraftInvoices => Set<DraftInvoice>();
    public DbSet<DraftInvoiceLine> DraftInvoiceLines => Set<DraftInvoiceLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<InvoiceFieldChangeHistory> ChangeHistory => Set<InvoiceFieldChangeHistory>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DepartmentAccess> DepartmentAccesses => Set<DepartmentAccess>();
    public DbSet<AggregationGroup> AggregationGroups => Set<AggregationGroup>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();
    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
            {
                dataSourceBuilder.ConnectionStringBuilder.ApplicationName = "Invoices-Web-Service";
                dataSourceBuilder.ConnectionStringBuilder.SslMode = SslMode.Require;
                dataSourceBuilder.ConnectionStringBuilder.Multiplexing = true;
                dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 150;
                dataSourceBuilder.ConnectionStringBuilder.KeepAlive = 30;
            });
            
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 2,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
    }
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasDefaultSchema("public");
        
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        b.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceLineConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceFieldChangeHistoryConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DraftInvoiceConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DraftInvoiceLineConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DepartmentAccessConfiguration).Assembly);
        
        b.Entity<Invoice>().HasQueryFilter(e => 
            EF.Property<Guid>(e, "DepartmentId") == GetCurrentDepartmentId());
    }

    private static Guid GetCurrentDepartmentId() => 
        throw new NotImplementedException();
}