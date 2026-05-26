using InvoicesWebService.Infrastructure.Configurations;
using Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared;
using Shared.Configurations;

namespace InvoicesWebService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<DraftInvoice> DraftInvoices => Set<DraftInvoice>();
    public DbSet<DraftInvoiceLine> DraftInvoiceLines => Set<DraftInvoiceLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<InvoiceFieldChangeHistory> ChangeHistory => Set<InvoiceFieldChangeHistory>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();

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
        
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        b.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(ProcessingErrorConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(RawTransactionConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceLineConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(InvoiceFieldChangeHistoryConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DraftInvoiceConfiguration).Assembly);
        b.ApplyConfigurationsFromAssembly(typeof(DraftInvoiceLineConfiguration).Assembly);
        
        b.Entity<Invoice>().HasQueryFilter(e => 
            EF.Property<Guid>(e, "DepartmentId") == GetCurrentDepartmentId());
        b.Entity<InvoiceLine>().HasQueryFilter(l => 
            EF.Property<Guid>(l.Invoice, "DepartmentId") == GetCurrentDepartmentId());
        
        b.Ignore<Counterparty>();
        b.Ignore<AggregationGroup>();
        b.Ignore<User>();
        b.Ignore<DepartmentAccess>();
        b.Ignore<ExportRecord>();
    }

    private static Guid GetCurrentDepartmentId() => 
        throw new NotImplementedException();
}