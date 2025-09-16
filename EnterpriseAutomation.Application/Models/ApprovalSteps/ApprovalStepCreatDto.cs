using EnterpriseAutomation.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.ApprovalSteps
{
    public class ApprovalStepCreatDto
    {
        public int StepId { get; set; } = default!;

        public int RequestId { get; set; } = default!;

        public Guid? ApproverUserId { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime? ApprovedAt { get; set; }
    }
}
