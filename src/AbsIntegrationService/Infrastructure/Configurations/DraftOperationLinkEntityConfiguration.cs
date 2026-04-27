using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class DraftOperationLinkEntityConfiguration(string schema) : IEntityTypeConfiguration<DraftOperationLinkEntity>
{
    public void Configure(EntityTypeBuilder<DraftOperationLinkEntity> builder)
    {
        builder.ToTable("draft_operation_links", schema);
        builder.HasKey(e => e.Id);
        
        builder
            .HasOne(l => l.InvoiceDraft)
            .WithMany(i => i.LinkedOperations)
            .HasForeignKey(l => l.InvoiceDraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}