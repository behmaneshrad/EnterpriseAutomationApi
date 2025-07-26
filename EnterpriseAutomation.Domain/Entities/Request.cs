using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Domain.Entities.Enums;
namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "موجودیت درخواست های کاربران")]
    public class Request : BaseEntity
    {
        [Key]
        public int RequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; } = default!;
        //public DateTime CreatedAt { get; set; } = default!;
        public RequestStatus CurrentStatus { get; set; } = RequestStatus.Pending;
        public string CurrentStep { get; set; } = default!;
        public int WorkflowId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<ApprovalStep> ApprovalSteps { get; set; }
    }
}