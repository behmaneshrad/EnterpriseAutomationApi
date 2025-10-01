using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions","Auth");

        builder.HasKey(p => p.PermissionId);      

        builder.Property(p => p.Name)
               .HasMaxLength(200);       

        builder.HasMany(p => p.Roles)
               .WithOne(r => r.Permission)
               .HasForeignKey(r => r.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
