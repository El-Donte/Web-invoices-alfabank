using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace AbsIntegrationService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("department");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        b.Property(e => e.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        
        b.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("UX_department_code");
    }
}