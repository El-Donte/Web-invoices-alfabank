using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class ProcessingErrorConfiguration : IEntityTypeConfiguration<ProcessingError>
{
    public void Configure(EntityTypeBuilder<ProcessingError> b)
    {
        b.ToTable("processing_error");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Stage).HasConversion<string>().HasColumnName("stage").HasMaxLength(32).IsRequired();
        b.Property(e => e.Code).HasColumnName("code").HasMaxLength(64).IsRequired();
        b.Property(e => e.Message).HasColumnName("message").HasColumnType("text").IsRequired();
        b.Property(e => e.Payload).HasColumnType("jsonb").HasColumnName("payload").IsRequired();
        b.Property(e => e.Retryable).HasColumnName("retryable").IsRequired().HasDefaultValue(true);
        b.Property(e => e.Resolved).HasColumnName("resolved").IsRequired().HasDefaultValue(false);
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.AggregationGroupId).HasColumnName("aggregation_group_id");
        b.Property(e => e.DraftInvoiceId).HasColumnName("draft_invoice_id");
        b.Property(e => e.InvoiceId).HasColumnName("invoice_id");
        b.Property(e => e.RawTransactionId).HasColumnName("raw_transaction_id");

        b.HasIndex(e => new { e.Resolved, e.Retryable, e.CreatedAt })
            .HasDatabaseName("IX_processing_error_retries");
        
        b.HasOne<AggregationGroup>()
            .WithMany()
            .HasForeignKey(e => e.AggregationGroupId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne<DraftInvoice>()
            .WithMany().
            HasForeignKey(e => e.DraftInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne<RawTransaction>()
            .WithMany()
            .HasForeignKey(e => e.RawTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}