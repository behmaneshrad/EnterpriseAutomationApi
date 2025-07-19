using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    public class WorkflowStep
    {
        public string StepId { get; set; } = default!;
        public string WorkflowId { get; set; } = default!;
        public int Order { get; set; } = default!;
        public string StepName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public bool Editable { get; set; } = default!;

        public WorkflowDefinition WorkflowDefinition { get; set; }
    }
}
