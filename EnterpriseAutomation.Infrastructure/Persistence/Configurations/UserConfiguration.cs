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

            // Relation to Requests model

            //builder.HasMany(r => r.Requests)
            //    .WithOne(u => u.CreatedByUser)
            //    .HasForeignKey(r => r.CreatedByUserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Relation to  WorkflowDefinition
            builder.HasMany(r => r.WorkflowDefinitions)
                .WithOne(u => u.User)
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            //// Relation To Approval
            //builder.HasMany(x=>x.ApprovalSteps)
            //    .WithOne(u => u.ApproverUser)
            //    .HasForeignKey(x=>x.ApproverUserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity("UserRole");
        }
    }
}
