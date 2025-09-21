using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations;

public class WorkflowLogConfiguration : IEntityTypeConfiguration<WorkflowLog>
{
    public void Configure(EntityTypeBuilder<WorkflowLog> builder)
    {
        // جدول + شِما
        builder.ToTable("WorkflowLogs", "Workflow");

        // کلید اصلی
        builder.HasKey(x => x.Id);

        // پراپرتی‌ها
        builder.Property(x => x.WorkflowId)
            .IsRequired();

        builder.Property(x => x.StepId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired(false); // چون Guid? هست

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.RequestId)
            .IsRequired();

        builder.Property(x => x.PreviousState)
            .HasMaxLength(200);

        builder.Property(x => x.NewState)
            .HasMaxLength(200);

        // ایندکس‌ها (برای سرعت جستجو لاگ‌ها)
        builder.HasIndex(x => x.WorkflowId);
        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.StepId);
    }
}
