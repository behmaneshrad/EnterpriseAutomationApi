using EnterpriseAutomation.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseAutomation.Domain.Entities.Enums;
namespace EnterpriseAutomation.Domain.Entities
{
    [Display(Name = "مراحل تایید")]
    public class ApprovalStep : BaseEntity
    {
        [Key]
        [Display(Name ="آی دی مراحل تایید")]
        public int ApprovalStepId { get; set; }

        [Display(Name = "شماره مرحله")]
        public int StepId { get; set; } = default!;

        [Display(Name = "آی دی درخواست")]
        public int RequestId { get; set; } = default!;

        [Display(Name = "آی دی کاربر تایید کننده")]
        public Guid? ApproverUserId { get; set; }

        [Display(Name = "وضعیت")]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Display(Name = "تاریخ تایید")]
        public DateTime? ApprovedAt { get; set; }

        public virtual Request? Request { get; set; }
    }
}
