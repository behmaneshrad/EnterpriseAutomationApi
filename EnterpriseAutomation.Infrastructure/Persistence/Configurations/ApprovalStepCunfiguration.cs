using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
    {
        public void Configure(EntityTypeBuilder<ApprovalStep> builder)
        {
            builder.HasKey(x => x.ApprovalStepId);

            builder.Property(x => x.StepId)
                .IsRequired();

            builder.Property(x => x.RequestId)
                .IsRequired();

            builder.Property(x => x.ApproverUserId)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.ApprovedAt)
                .IsRequired(false); // nullable - زمانی که هنوز تایید نشده

            // Relation to Request
            builder.HasOne(x => x.Request)
                .WithMany(r => r.ApprovalSteps)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation to User (Approver)
            builder.HasOne(x => x.User)
                .WithMany(u => u.ApprovalSteps)
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}