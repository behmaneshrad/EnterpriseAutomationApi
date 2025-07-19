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
    public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
    {
        public void Configure(EntityTypeBuilder<WorkflowStep> builder)
        {
            builder.HasKey(x => x.StepId);

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

            // Relation to WorkflowDefinition
            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany(y => y.WorkflowSteps)
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
