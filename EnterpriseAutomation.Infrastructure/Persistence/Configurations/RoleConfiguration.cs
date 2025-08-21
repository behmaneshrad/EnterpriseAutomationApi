using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        // کلید اصلی
        builder.HasKey(r => r.RoleId);

        // خصوصیات
        builder.Property(r => r.RoleName)
               .IsRequired()
               .HasMaxLength(200);

        // ایندکس یکتا برای نام نقش
        builder.HasIndex(r => r.RoleName).IsUnique();

    }
}