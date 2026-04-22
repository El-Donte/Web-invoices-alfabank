using AbsIntegrationService.Infrastructure.Configurations;
using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration): DbContext(options)
{
    public DbSet<InvoiceDraftEntity> InvoicesDrafts { get; set; }
    
    public DbSet<InvoiceDraftLineEntity> InvoiceLines { get; set; }
    
    public DbSet<DraftOperationLinkEntity> OperationLinks { get; set; }
    private string _schema = "";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetSection("UseConnection").Value;
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
        modelBuilder.Entity<InvoiceDraftEntity>(entity =>
        {
            entity.ToTable("invoice_drafts", _schema);
        });
        
        modelBuilder.Entity<InvoiceDraftLineEntity>(entity =>
        {
            entity.ToTable("invoice_draft_lines", _schema);
        });
        
        modelBuilder.Entity<DraftOperationLinkEntity>(entity =>
        {
            entity.ToTable("draft_operation_links", _schema);
        });
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new InvoiceDraftEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceDraftLineEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}