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
    [Display(Name = "درخواست")]
    public class Request : BaseEntity
    {
        [Key]
        [Display(Name ="آی دی درخواست")]
        public int RequestId { get; set; }

        [Display(Name = "تیتر درخواست")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "شرح درخواست")]
        public string Description { get; set; } = string.Empty;


        // نقض اصل DRY 
        [Display(Name = "آی دی کاربر سازنده")]
        public Guid CreatedByUserId { get; set; } = default!;

        //[Display(Name = "آی دی کاربر تغییر دهنده")]
        //public int ModifiedByUserId { get; set; } = default!;
        //public DateTime CreatedAt { get; set; } = default!;

        [Display(Name = "وضعیت فعلی")]
        public RequestStatus CurrentStatus { get; set; } = RequestStatus.Pending;

        [Display(Name = "مرحله فعلی")]
        public string CurrentStep { get; set; } = default!;

        [Display(Name = "آی دی شرح گردش کار")]
        public int WorkflowDefinitionId { get; set; }


        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
        //public virtual User? CreatedByUser { get; set; }
        //public virtual User? ModifiedByUser { get; set; }

        public virtual ICollection<ApprovalStep> ApprovalSteps { get; set; } = [];
       
    }
}