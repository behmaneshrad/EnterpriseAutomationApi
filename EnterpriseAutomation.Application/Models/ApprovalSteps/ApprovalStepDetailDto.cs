using EnterpriseAutomation.Domain.Entities.Enums;

namespace EnterpriseAutomation.Application.Models.ApprovalSteps
{
    public class ApprovalStepDetailDto
    {
        public int ApprovalStepId { get; set; }

        public int StepId { get; set; } = default!;

        public int RequestId { get; set; } = default!;

        public Guid? ApproverUserId { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid UserCreatedId { get; set; }

        public Guid? UserModifyId { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
