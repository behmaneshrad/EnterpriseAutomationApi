using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", "Auth");

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.RefreshToken)
                .HasMaxLength(2000);

            builder.Property(x => x.KeycloakId).HasMaxLength(50).HasColumnType("nvarchar").IsRequired(false);

          
            builder.HasMany(r => r.WorkflowDefinitions)
                .WithOne(u => u.User)
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity("UserRole");
        }
    }
}
