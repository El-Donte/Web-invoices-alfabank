using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class DepartmentAccessConfiguration : IEntityTypeConfiguration<DepartmentAccess>
{
    public void Configure(EntityTypeBuilder<DepartmentAccess> b)
    {
        b.ToTable("department_access");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id");
        b.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        b.Property(e => e.DepartmentId).HasColumnName("department_id").IsRequired();
        
        b.HasIndex(e => new { e.UserId, e.DepartmentId })
            .IsUnique()
            .HasDatabaseName("UX_department_access_user_dept");
        
        b.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_department_access_user");

        b.HasOne<User>()
            .WithMany(u => u.DepartmentAccesses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        b.HasOne<Department>()
            .WithMany(d => d.DepartmentAccesses)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}