using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesAlfa.Infrastructure.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> b)
    {
        b.ToTable("invoice_line");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255).IsRequired();
        b.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(255).IsRequired();
        b.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(32).IsRequired();
        b.Property(e => e.Quantity).HasPrecision(16, 4).HasColumnName("quantity").IsRequired();
        b.Property(e => e.UnitPrice).HasPrecision(16, 4).HasColumnName("unit_price").IsRequired();
        b.Property(e => e.AmountWithoutNds).HasPrecision(16, 4).HasColumnName("amount_without_nds").IsRequired();
        b.Property(e => e.AmountWithNds).HasPrecision(16, 4).HasColumnName("amount_with_nds").IsRequired();
        b.Property(e => e.NdsAmount).HasPrecision(16, 4).HasColumnName("nds_amount").IsRequired();
        b.Property(e => e.NdsRate).HasPrecision(16, 4).HasColumnName("nds_rate").IsRequired();
        b.Property(e => e.TotalAmount).HasPrecision(16, 4).HasColumnName("total_amount").IsRequired();
        b.Property(e => e.InvoiceId).HasColumnName("invoice_id").IsRequired();

        b.HasIndex(e => e.InvoiceId)
            .HasDatabaseName("IX_invoice_line_invoice");
        
        b.HasOne<Invoice>()
            .WithMany(i => i.Lines)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}