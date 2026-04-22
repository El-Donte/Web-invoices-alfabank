using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class InvoiceDraftEntityConfiguration : IEntityTypeConfiguration<InvoiceDraftEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceDraftEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceDraftId);
        
        builder
            .HasMany(d => d.LinkedOperations)
            .WithOne()
            .HasForeignKey(l => l.InvoiceDraftId);
        
        builder
            .HasIndex(d => d.OperationNumber)
            .IsUnique();
    }
}