using AbsIntegrationService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class InvoiceDraftLineEntityConfiguration : IEntityTypeConfiguration<InvoiceDraftLineEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceDraftLineEntity> builder)
    {
        builder.HasIndex(l => l.InvoiceDraftId);
    }
}