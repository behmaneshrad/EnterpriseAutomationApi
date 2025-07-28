using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "موجودیت مرحله گردش کار")]
    public class WorkflowStep
    {
        [Key]
        public int WorkflowStepId { get; set; }
        public int WorkflowId { get; set; } = default!;
        public int Order { get; set; } = default!;
        public string StepName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Editable { get; set; }

        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        public virtual ICollection<Request> Requests { get; set; } = [];
    }
}
