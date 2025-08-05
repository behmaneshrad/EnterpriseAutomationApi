using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Models
{
    public class WorkflowDefinitionAndWorkflowStepDto
    {
        public int WorkflowDefinitionId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedById { get; set; }

        public int UpdatedById { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<WorkflowStepDto> WorkflowStepDto { get; set; } = [];
    }

    public class WorkflowStepDto
    {
        public int WorkflowStepId { get; set; }

        public int WorkflowDefinitionId { get; set; }

        public int Order { get; set; }

        public string StepName { get; set; }

        public string Role { get; set; }

        public bool Editable { get; set; }
    }
}
