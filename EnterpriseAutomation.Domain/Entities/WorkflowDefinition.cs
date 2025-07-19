using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities
{
    public class WorkflowDefinition
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;

        public User User { get; set; }
    }
}
