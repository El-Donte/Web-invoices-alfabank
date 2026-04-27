using AbsIntegrationService.Infrastructure.Configurations;
using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AbsIntegrationService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration): DbContext(options)
{
    public DbSet<InvoiceDraftEntity> InvoicesDrafts { get; set; }
    
    public DbSet<InvoiceDraftLineEntity> InvoiceLines { get; set; }
    
    public DbSet<DraftOperationLinkEntity> OperationLinks { get; set; }
    private string _schema = "";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionKeyName = configuration.GetSection("UseConnection").Value!;
        var connectionString = configuration.GetConnectionString(connectionKeyName);
        _schema = configuration.GetSection("Schema").Value!;

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(120);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema); 
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new InvoiceDraftEntityConfiguration(_schema));
        modelBuilder.ApplyConfiguration(new InvoiceDraftLineEntityConfiguration(_schema));
        modelBuilder.ApplyConfiguration(new DraftOperationLinkEntityConfiguration(_schema));
        
        base.OnModelCreating(modelBuilder);
    }
}