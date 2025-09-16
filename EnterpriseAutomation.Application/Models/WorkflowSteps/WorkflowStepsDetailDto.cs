using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.WorkflowSteps
{
    public class WorkflowStepsDetailDto
    {
        public int WorkflowStepId { get; set; } = default!;

        public int WorkflowDefinitionId { get; set; } = default!;

        public int Order { get; set; } = default!;

        public string StepName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool Editable { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid UserCreatedId { get; set; }

        public Guid? UserModifiedId { get; set; }
    }
}
