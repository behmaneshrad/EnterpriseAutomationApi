﻿using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired();

            builder.Property(t => t.Description)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            // Relation to user 
            builder.HasOne(t=>t.User)
                .WithMany(u=>u.WorkflowDefinitions)
                .HasForeignKey(t=>t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
