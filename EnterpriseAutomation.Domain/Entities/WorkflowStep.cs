using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{

    [Display(Name = "مرحله گردش کار")]
    public class WorkflowStep
    {
        [Key]
        [Display(Name ="آی دی مرحله گردش کار")]
        public int WorkflowStepId { get; set; }

        [Display(Name = "آی دی شرح گردش کار")]
        public int WorkflowDefinitionId { get; set; } = default!;

        [Display(Name ="مرحله ترتیب")]
        public int Order { get; set; } = default!;

        [Display(Name ="نام مرحله")]
        public string StepName { get; set; } = string.Empty;

        [Display(Name ="نقش")]
        public string Role { get; set; } = string.Empty;

        [Display(Name ="ویرایش پذیری")]
        public bool Editable { get; set; }

        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

    }
}
