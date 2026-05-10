using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class DraftInvoiceLineConfiguration : IEntityTypeConfiguration<DraftInvoiceLine>
{
    public void Configure(EntityTypeBuilder<DraftInvoiceLine> b)
    {
        b.ToTable("draft_invoice_line");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id").IsRequired();
        b.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255).IsRequired();
        b.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(255).IsRequired();
        b.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(32).IsRequired();
        b.Property(e => e.Quantity).HasPrecision(16, 4).HasColumnName("quantity").IsRequired();
        b.Property(e => e.UnitPrice).HasPrecision(16, 4).HasColumnName("unit_price").IsRequired();
        b.Property(e => e.AmountWithoutNds).HasPrecision(16, 4).HasColumnName("amount_without_nds").IsRequired();
        b.Property(e => e.NdsRate).HasPrecision(16, 4).HasColumnName("nds_rate").IsRequired();
        b.Property(e => e.NdsAmount).HasPrecision(16, 4).HasColumnName("nds_amount").IsRequired();
        b.Property(e => e.TotalAmount).HasPrecision(16, 4).HasColumnName("total_amount").IsRequired();
        b.Property(e => e.DraftInvoiceId).HasColumnName("draft_invoice_id").IsRequired();
        b.Property(e => e.RawTransactionId).HasColumnName("raw_transaction_id");

        b.HasIndex(e => e.DraftInvoiceId).HasDatabaseName("IX_draft_invoice_line_draft");
        b.HasIndex(e => e.RawTransactionId).IsUnique().HasDatabaseName("UX_draft_invoice_line_raw_tx");
        
        b.HasOne<RawTransaction>()
            .WithMany()
            .HasForeignKey(e => e.RawTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne(e => e.DraftInvoice)
            .WithMany(i => i.Lines)
            .HasForeignKey(l => l.DraftInvoiceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}