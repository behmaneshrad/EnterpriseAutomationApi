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
    public class ApprovalStepCunfiguration : IEntityTypeConfiguration<ApprovalStep>
    {
        public void Configure(EntityTypeBuilder<ApprovalStep> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StepId)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.ApprovedAt)
                .IsRequired();

            // Relation to user
            builder.HasOne(x => x.User)
                .WithMany(u => u.ApprovalSteps)
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation to Request
            builder.HasOne(x => x.Request)
                .WithMany(r => r.ApprovalSteps)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
