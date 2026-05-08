using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Entities;

namespace ExportService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> opts, IConfiguration configuration) : DbContext
{
    public DbSet<ExportRecord> ExportRecords => Set<ExportRecord>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>(); 
    
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
            {
                dataSourceBuilder.ConnectionStringBuilder.ApplicationName = "Export-Service";
                dataSourceBuilder.ConnectionStringBuilder.SslMode = SslMode.Require;
                dataSourceBuilder.ConnectionStringBuilder.Multiplexing = true;
                dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 150;
                dataSourceBuilder.ConnectionStringBuilder.KeepAlive = 30;
            });
            
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
    }
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

    }
}