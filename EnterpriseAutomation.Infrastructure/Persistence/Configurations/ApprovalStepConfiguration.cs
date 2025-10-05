using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
    {
        public void Configure(EntityTypeBuilder<ApprovalStep> builder)
        {
            builder.ToTable("ApprovalSteps", "workflow");

            builder.HasKey(x => x.ApprovalStepId);

            builder.Property(x => x.StepId).IsRequired();
            builder.Property(x => x.RequestId).IsRequired();

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<int>(); // شفاف‌سازی ذخیره‌ی enum

            builder.Property(x => x.ApprovedAt).IsRequired(false);

            // ایندکس‌ها
            builder.HasIndex(x => x.RequestId);
            builder.HasIndex(x => x.ApproverUserId);

            // جلوگری از تکرار شماره مرحله برای یک درخواست
            builder.HasIndex(x => new { x.RequestId, x.StepId })
                   .IsUnique();

            // Request ↔ ApprovalSteps
            builder.HasOne(x => x.Request)
                   .WithMany(r => r.ApprovalSteps)
                   .HasForeignKey(x => x.RequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ApproverUser ↔ ApprovalSteps  (FK: int? → Users.UserId)
            builder.HasOne(x => x.ApproverUser)
                   .WithMany(u => u.ApprovalSteps)
                   .HasForeignKey(x => x.ApproverUserId)
                   .OnDelete(DeleteBehavior.Restrict); // یا SetNull
                                                       // .OnDelete(DeleteBehavior.SetNull);
        }
    }
}