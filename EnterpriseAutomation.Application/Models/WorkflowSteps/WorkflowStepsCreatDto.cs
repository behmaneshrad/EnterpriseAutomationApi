using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.WorkflowSteps
{
    public class WorkflowStepsCreatDto
    {
        public int WorkflowDefinitionId { get; set; } = default!;

        
        public int Order { get; set; } = default!;

        
        public string StepName { get; set; } = string.Empty;

        
        public string Role { get; set; } = string.Empty;

        
        public bool Editable { get; set; }
    }
}
