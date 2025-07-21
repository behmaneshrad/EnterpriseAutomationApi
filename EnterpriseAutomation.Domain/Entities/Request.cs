using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseAutomation.Domain.Entities.Base;
namespace EnterpriseAutomation.Domain.Entities
{
    public class Request:Base
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string CreatedByUserId { get; set; } = default!;
        //public DateTime CreatedAt { get; set; } = default!;
        public string CurrentStatus { get; set; } = default!;
        public string CurrentStep { get; set; } = default!;
        public string WorkflowId { get; set; } = default!;

        public User User { get; set; }
        public ICollection<ApprovalStep> ApprovalSteps { get; set; }
    }
}