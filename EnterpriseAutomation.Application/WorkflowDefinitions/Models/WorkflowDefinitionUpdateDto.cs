using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Models
{
    public class WorkflowDefinitionUpdateDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int UpdatedById { get; set; }

        public DateTime UpdatedAt { get; set; }

        public static WorkflowDefinition MapFrom(
            WorkflowDefinitionUpdateDto dto, WorkflowDefinition workflowDefinition)
        {
            workflowDefinition.Description = dto.Description;
            workflowDefinition.Name = dto.Name;
            workflowDefinition.UserModifyId = dto.UpdatedById;
            workflowDefinition.UpdatedAt = dto.UpdatedAt;

            return workflowDefinition;
        }
    }
}
