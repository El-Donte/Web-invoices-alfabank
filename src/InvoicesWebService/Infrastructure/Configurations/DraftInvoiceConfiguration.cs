using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class DraftInvoiceConfiguration : IEntityTypeConfiguration<DraftInvoice>
{
    public void Configure(EntityTypeBuilder<DraftInvoice> b)
    {
        b.ToTable("draft_invoice");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.TransactionDate).HasColumnName("transaction_date").IsRequired();
        b.Property(e => e.NdsRate).HasPrecision(6, 4).HasColumnName("nds_rate").IsRequired();
        b.Property(e => e.TotalNdsAmount).HasPrecision(16, 4).HasColumnName("total_nds_amount").IsRequired();
        b.Property(e => e.TotalWithNds).HasPrecision(16, 4).HasColumnName("total_with_nds").IsRequired();
        b.Property(e => e.CurrencyCode).HasColumnName("currency_code").IsRequired();
        b.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(32).IsRequired();
        b.Property(e => e.ValidationError).HasColumnName("validation_error").HasColumnType("text").IsRequired();
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
        b.Property(e => e.DepartmentId).HasColumnName("department_id").IsRequired();
        b.Property(e => e.AggregationGroupId).HasColumnName("group_id").IsRequired();

        b.HasIndex(e => e.AggregationGroupId)
            .IsUnique()
            .HasDatabaseName("UX_draft_invoice_group_id");

        b.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<Department>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<AggregationGroup>()
            .WithMany()
            .HasForeignKey(e => e.AggregationGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasMany<DraftInvoiceLine>()
            .WithOne() 
            .HasForeignKey(l => l.DraftInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}