using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    public class ApprovalStep
    {

        public string Id { get; set; } = default!;
        public int StepId { get; set; } = default!;

        public string RequestId { get; set; } = default!;

        public string ApproverUserId { get; set; } = default!;

        public string Status { get; set; } = default!;

        public DateTime ApprovedAt { get; set; } = default!;

        public User User { get; set; }
        public Request Request { get; set; }
    }
}
