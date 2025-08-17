using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(p => p.Name)
               .HasMaxLength(200);

        builder.Property(p => p.Description)
               .HasMaxLength(1000);

        builder.Property(p => p.Version)
               .IsRequired();

        builder.Property(p => p.IsActive)
               .IsRequired();

        builder.HasIndex(p => new { p.Key, p.Version }).IsUnique();
        builder.HasIndex(p => new { p.Key, p.IsActive });

        builder.HasMany(p => p.Roles)
               .WithOne(r => r.Permission)
               .HasForeignKey(r => r.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
