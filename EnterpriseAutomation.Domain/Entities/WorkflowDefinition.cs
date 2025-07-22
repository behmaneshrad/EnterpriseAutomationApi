using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "موجودیت گردش کارها")]
    public class WorkflowDefinition
    {
        [Key]
        public int WorkflowDefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
       // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }
}
