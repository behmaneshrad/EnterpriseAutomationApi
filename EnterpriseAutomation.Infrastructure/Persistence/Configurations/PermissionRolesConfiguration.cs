using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class PermissionRolesConfiguration : IEntityTypeConfiguration<PermissionRole>
{
    public void Configure(EntityTypeBuilder<PermissionRole> builder)
    {
        builder.ToTable("PermissionRoles");

        builder.HasKey(r => r.PermissionId);

        builder.Property(r => r.RoleName)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasIndex(r => new { r.PermissionId, r.RoleName }).IsUnique();
    }
}
