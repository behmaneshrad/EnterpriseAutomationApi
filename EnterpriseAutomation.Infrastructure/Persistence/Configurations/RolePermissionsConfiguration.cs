using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class RolePermissionsConfiguration : IEntityTypeConfiguration<RolePermissions>
{
    public void Configure(EntityTypeBuilder<RolePermissions> builder)
    {
        builder.ToTable("RolesPermissions");

        builder.HasKey(r => r.RolePermissionsId);

        builder.Property(r => r.RoleId)
               .IsRequired();

        builder.Property(r => r.PermissionId)
               .IsRequired();

        builder.HasIndex(r => new { r.PermissionId, r.RoleId }).IsUnique();

        builder.HasOne(r => r.Role)
               .WithMany()
               .HasForeignKey(r => r.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Permission)
               .WithMany(p => p.Roles)
               .HasForeignKey(r => r.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

