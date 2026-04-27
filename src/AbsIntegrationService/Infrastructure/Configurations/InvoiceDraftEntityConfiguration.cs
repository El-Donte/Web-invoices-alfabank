using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class InvoiceDraftEntityConfiguration(string schema) : IEntityTypeConfiguration<InvoiceDraftEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceDraftEntity> builder)
    {
        builder.ToTable("invoice_drafts", schema);
        builder.HasKey(e => e.Id);

        builder
            .HasMany(e => e.Lines)
            .WithOne(l => l.InvoiceDraft)
            .HasForeignKey(l => l.InvoiceDraftId);
        
        builder
            .HasMany(d => d.LinkedOperations)
            .WithOne(l => l.InvoiceDraft)
            .HasForeignKey(l => l.InvoiceDraftId);
        
        builder
            .HasIndex(d => d.OperationNumber)
            .IsUnique();
    }
}