using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("invoice");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id").IsRequired();
        b.Property(e => e.Number).HasColumnName("number").HasMaxLength(64).IsRequired();
        b.Property(e => e.IssueDate).HasColumnName("issue_date").IsRequired();
        b.Property(e => e.NdsRate).HasPrecision(6, 4).HasColumnName("nds_rate").IsRequired();
        b.Property(e => e.TotalNds).HasPrecision(16, 4).HasColumnName("total_nds").IsRequired();
        b.Property(e => e.TotalWithNds).HasPrecision(16, 4).HasColumnName("total_with_nds").IsRequired();
        b.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(32).IsRequired();
        b.Property(e => e.PaymentDocDate).HasColumnName("payment_doc_date");
        b.Property(e => e.PaymentDocNumber).HasColumnName("payment_doc_number").HasMaxLength(128).IsRequired();
        b.Property(e => e.SequenceNumber).HasColumnName("sequence_number").HasMaxLength(64).IsRequired();
        b.Property(e => e.CurrentVersion).HasColumnName("current_version").IsRequired().HasDefaultValue(1);
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
        b.Property(e => e.DepartmentId).HasColumnName("department_id").IsRequired();
        b.Property(e => e.DraftInvoiceId).HasColumnName("draft_invoice_id");
        b.Property(e => e.LastAuthorId).HasColumnName("last_author_id");

        b.HasIndex(e => e.Number)
            .IsUnique()
            .HasDatabaseName("UX_invoice_number");
        
        b.HasIndex(e => new { e.Status, e.IssueDate })
            .HasDatabaseName("IX_invoice_status_date");
        
        b.HasIndex(e => e.DraftInvoiceId)
            .IsUnique()
            .HasDatabaseName("UX_invoice_draft_id");

        b.Property(e => e.CurrentVersion)
            .IsConcurrencyToken();

        b.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<Department>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<DraftInvoice>()
            .WithMany()
            .HasForeignKey(e => e.DraftInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.LastAuthorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}