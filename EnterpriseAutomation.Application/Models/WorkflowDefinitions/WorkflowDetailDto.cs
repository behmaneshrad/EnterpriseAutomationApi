using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.WorkflowDefinitions
{
    public class WorkflowDetailDto
    {
        public int WorkflowDefinitionId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedById { get; set; }

        public int UpdatedById { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
