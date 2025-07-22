using EnterpriseAutomation.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "موجودیت مراحل تایید")]
    public class ApprovalStep : BaseEntity
    {
        [Key]
        public int ApprovalStepId { get; set; }
        public int StepId { get; set; } = default!;

        public int RequestId { get; set; } = default!;

        public int ApproverUserId { get; set; } = default!;

        public string Status { get; set; } = String.Empty;

        public DateTime ApprovedAt { get; set; } = default!;

        public virtual User User { get; set; }
        public virtual Request Request { get; set; }
    }
}
