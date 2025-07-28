using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "شرح گردش کار")]
    public class WorkflowDefinition
    {
        [Key]
        [Display(Name = "آی دی شرح گردش کار")]
        public int WorkflowDefinitionId { get; set; }

        [Display(Name = "نام گردش کار")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "شرح گردش کار")]
        public string Description { get; set; } = string.Empty;
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "آی دی سازنده گردش کار")]
        public int CreatedById { get; set; }

        public virtual User? User { get; set; }

        public virtual ICollection<WorkflowStep> WorkflowSteps { get; set; } = [];

        public virtual ICollection<Request> Requests { get; set; } = [];
    }
}
