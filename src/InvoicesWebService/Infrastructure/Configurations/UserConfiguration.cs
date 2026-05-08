using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesAlfa.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("user");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id");
        b.Property(e => e.Login).HasColumnName("login").IsRequired();
        b.Property(e => e.Password).HasColumnName("password").IsRequired();
        b.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(128).IsRequired();
        b.Property(e => e.Position).HasColumnName("position");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}