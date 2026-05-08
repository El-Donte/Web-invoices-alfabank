using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> b)
    {
        b.ToTable("counterparty");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        b.Property(e => e.Inn).HasColumnName("inn").HasMaxLength(16).IsRequired();
        b.Property(e => e.Kpp).HasColumnName("kpp").HasMaxLength(16).IsRequired();
        b.Property(e => e.Address).HasColumnName("address").HasMaxLength(255).IsRequired();
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        
        b.HasIndex(e => e.Inn)
            .HasDatabaseName("IX_counterparty_inn");
    }
}