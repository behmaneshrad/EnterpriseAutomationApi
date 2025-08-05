using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Models
{
    public class WorkflowDefinitionGetDto
    {
        public int WorkflowDefinitionId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedById { get; set; }

        public int UpdatedById { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public static WorkflowDefinitionGetDto MapFrom(WorkflowDefinition workflowDefinition)
        {
            return new WorkflowDefinitionGetDto
            {
                WorkflowDefinitionId = workflowDefinition.WorkflowDefinitionId,
                Name = workflowDefinition.Name,
                Description = workflowDefinition.Description,
                CreatedById = workflowDefinition.CreatedById,
                CreatedAt = workflowDefinition.CreatedAt,
                UpdatedAt = workflowDefinition.UpdatedAt ?? DateTime.MinValue,
                UpdatedById = workflowDefinition.UserModifyId ?? 0
            };
        }
    }
}
