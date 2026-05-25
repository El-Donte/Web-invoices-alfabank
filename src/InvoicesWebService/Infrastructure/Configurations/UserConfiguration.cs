using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("user");
        b.HasKey(e => e.Id);
        
        b.Property(e => e.Id).HasColumnName("id");
        b.Property(e => e.UserName).HasColumnName("login").IsRequired();
        b.Property(e => e.PasswordHash).HasColumnName("password").IsRequired();
        b.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        b.Property(e => e.Position).HasColumnName("position");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}