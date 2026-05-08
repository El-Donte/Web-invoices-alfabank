using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class RawTransactionConfiguration : IEntityTypeConfiguration<RawTransaction>
{
    public void Configure(EntityTypeBuilder<RawTransaction> b)
    {
        b.ToTable("raw_transaction");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.OperationNumber).HasColumnName("operation_number").HasMaxLength(64).IsRequired();
        b.Property(e => e.Type).HasConversion<string>().HasColumnName("type").HasMaxLength(16).IsRequired();
        b.Property(e => e.Date).HasColumnName("date");
        b.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255);
        b.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(32).IsRequired();
        b.Property(e => e.UnitMeasure).HasColumnName("unit_measure").HasMaxLength(32);
        b.Property(e => e.Quantity).HasPrecision(16, 4).HasColumnName("quantity");
        b.Property(e => e.UnitPrice).HasPrecision(16, 4).HasColumnName("unit_price");
        b.Property(e => e.NdsRate).HasPrecision(6, 4).HasColumnName("nds_rate");
        b.Property(e => e.NdsAmount).HasPrecision(16, 4).HasColumnName("nds_amount");
        b.Property(e => e.Amount).HasPrecision(16, 4).HasColumnName("amount");
        b.Property(e => e.PayloadHash).HasColumnName("payload_hash").HasMaxLength(64).IsRequired();
        b.Property(e => e.Payload).HasColumnType("jsonb").HasColumnName("payload").IsRequired();
        b.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(32).IsRequired();
        b.Property(e => e.ValidationError).HasColumnName("validation_error").HasColumnType("text").IsRequired();
        b.Property(e => e.ReceivedAt).HasColumnName("received_at");
        b.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.AggregationGroupId).HasColumnName("aggregation_group_id");
        b.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
        b.Property(e => e.DepartmentId).HasColumnName("department_id");
        b.Property(e => e.InvoiceId).HasColumnName("invoice_id");

        b.HasIndex(e => e.PayloadHash)
            .IsUnique()
            .HasDatabaseName("UX_rawtransaction_payload_hash");
        
        b.HasIndex(e => new { e.Status, e.ReceivedAt })
            .HasDatabaseName("IX_rawtransaction_status_received");
        
        b.HasIndex(e => new { e.OperationNumber, e.Type })
            .HasDatabaseName("IX_rawtransaction_drf_type");

        b.HasOne<AggregationGroup>()
            .WithMany()
            .HasForeignKey(e => e.AggregationGroupId)
            .OnDelete(DeleteBehavior.SetNull);
        
        b.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(e => e.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<Department>()
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}