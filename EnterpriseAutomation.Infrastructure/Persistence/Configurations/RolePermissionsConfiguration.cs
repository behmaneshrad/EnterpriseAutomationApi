using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class RolePermissionsConfiguration : IEntityTypeConfiguration<RolePermissions>
{
    public void Configure(EntityTypeBuilder<RolePermissions> builder)
    {
        builder.ToTable("RolesPermissions","Auth");

        // کلید اصلی
        builder.HasKey(rp => rp.RolePermissionsId);

        // خصوصیات scalar
        builder.Property(rp => rp.RoleId)
               .IsRequired();

        builder.Property(rp => rp.PermissionId)
               .IsRequired();

        // ایندکس ترکیبی یکتا
        builder.HasIndex(rp => new { rp.PermissionId, rp.RoleId })
               .IsUnique();

        // روابط
        builder.HasOne(rp => rp.Role)
               .WithMany(r => r.RolePermissions) // اضافه کردن collection در Role
               .HasForeignKey(rp => rp.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
               .WithMany(p => p.Roles)
               .HasForeignKey(rp => rp.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
