using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
    {
        public void Configure(EntityTypeBuilder<WorkflowStep> builder)
        {

            builder.ToTable("WorkflowSteps", "Workflow");

            // Primary Key
            builder.HasKey(x => x.WorkflowStepId);

            // WorkflowStep specific properties
            builder.Property(x => x.StepName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Editable)
                .IsRequired();

            builder.Property(x => x.WorkflowDefinitionId)
                .IsRequired();

            // BaseEntity properties configuration
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()"); // For SQL Server

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Now these can be nullable since they're int? in BaseEntity
            builder.Property(x => x.UserCreatedId)
                .IsRequired(false);

            builder.Property(x => x.UserModifyId)
                .IsRequired(false);

            // Relation to WorkflowDefinition (correct FK)
            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany(y => y.WorkflowSteps)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for better performance
            builder.HasIndex(x => x.WorkflowDefinitionId)
                .HasDatabaseName("IX_WorkflowStep_WorkflowDefinitionId");

            builder.HasIndex(x => new { x.WorkflowDefinitionId, x.Order })
                .HasDatabaseName("IX_WorkflowStep_WorkflowDefinitionId_Order");

            // Table name (optional)
            builder.ToTable("WorkflowSteps");
        }
    }
}