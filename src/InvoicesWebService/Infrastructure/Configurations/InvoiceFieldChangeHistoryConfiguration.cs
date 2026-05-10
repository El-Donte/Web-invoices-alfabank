using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class InvoiceFieldChangeHistoryConfiguration : IEntityTypeConfiguration<InvoiceFieldChangeHistory>
{
    public void Configure(EntityTypeBuilder<InvoiceFieldChangeHistory> b)
    {
        b.ToTable("invoice_field_change_history");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id").IsRequired();
        b.Property(e => e.FieldName).HasColumnName("field_name").HasMaxLength(128).IsRequired();
        b.Property(e => e.OldValue).HasColumnName("old_value").HasColumnType("text").IsRequired();
        b.Property(e => e.NewValue).HasColumnName("new_value").HasColumnType("text").IsRequired();
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.InvoiceId).HasColumnName("invoice_id").IsRequired();
        b.Property(e => e.ChangedById).HasColumnName("changed_by_id");

        b.HasIndex(e => e.InvoiceId)
            .HasDatabaseName("IX_invoice_change_history_invoice");
        
        b.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ChangedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}