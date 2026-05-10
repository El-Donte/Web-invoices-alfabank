using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace ExportService.Infrastructure.Configurations;

public class ExportRecordConfiguration : IEntityTypeConfiguration<ExportRecord>
{
    public void Configure(EntityTypeBuilder<ExportRecord> b)
    {
        b.ToTable("export_record");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(32).IsRequired();
        b.Property(e => e.Destination).HasColumnName("destination").HasMaxLength(64).IsRequired().HasDefaultValue("csv");
        b.Property(e => e.LastError).HasColumnName("last_error").HasColumnType("text").IsRequired();
        b.Property(e => e.LastAttemptAt).HasColumnName("last_attempt_at");
        b.Property(e => e.ExportedAt).HasColumnName("exported_at");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.InvoiceId).HasColumnName("invoice_id").IsRequired();

        b.HasIndex(e => e.InvoiceId)
            .IsUnique()
            .HasDatabaseName("UX_export_record_invoice");
        
        b.HasIndex(e => new { e.Status, e.LastAttemptAt })
            .HasDatabaseName("IX_export_record_status_retry");
        //
        // b.HasOne<Invoice>()
        //     .WithMany()
        //     .HasForeignKey(e => e.InvoiceId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}