using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            builder.ToTable("WorkflowDefinitions", "Workflow");

            builder.HasKey(t => t.WorkflowDefinitionId);

            builder.Property(t => t.Name)
                .IsRequired();

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(1000);

            // Relation to user 
            builder.HasOne(t => t.User)
                .WithMany(u => u.WorkflowDefinitions)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation to WorkflowStep (correct FK)
            builder.HasMany(x => x.WorkflowSteps)
                .WithOne(y => y.WorkflowDefinition)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
