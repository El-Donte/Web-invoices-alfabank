using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class AggregationGroupConfiguration : IEntityTypeConfiguration<AggregationGroup>
{
    public void Configure(EntityTypeBuilder<AggregationGroup> b)
    {
        b.ToTable("aggregation_group");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.ShipmentCount).HasColumnName("shipment_count");
        b.Property(e => e.AdvanceCount).HasColumnName("advance_count");
        b.Property(e => e.CorrectiveCount).HasColumnName("corrective_count");
        b.Property(e => e.TotalCount).HasColumnName("total_count");
        b.Property(e => e.OperationNumber).HasColumnName("operation_number").HasMaxLength(64).IsRequired();
        b.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(32).IsRequired();
        b.Property(e => e.TransactionDate).HasColumnName("transaction_date");
        b.Property(e => e.ValidationError).HasColumnName("validation_error").HasColumnType("text").IsRequired();
        b.Property(e => e.LastProcessedAt).HasColumnName("last_processed_at");
        b.Property(e => e.ReadyAt).HasColumnName("ready_at");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        b.Property(e => e.CounterpartyId).HasColumnName("counterparty_id");
        b.Property(e => e.DepartmentId).HasColumnName("department_id");

        b.HasIndex(e => e.OperationNumber)
            .IsUnique()
            .HasDatabaseName("UX_aggregation_group_drf");
        
        b.HasIndex(e => new { e.Status, e.TransactionDate })
            .HasDatabaseName("IX_aggregation_group_status_date");

        b.HasOne<Counterparty>().WithMany().HasForeignKey(e => e.CounterpartyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Department>().WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}