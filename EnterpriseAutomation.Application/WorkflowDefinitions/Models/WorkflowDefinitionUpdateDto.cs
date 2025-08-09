using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Models
{
    public class WorkflowDefinitionUpdateDto
    {
        public WorkflowDefinitionUpdateDto()
        {
            this.UpdatedAt = DateTime.Now;
        }
        public string Name { get; set; }

        public string Description { get; set; }

        public int UpdatedById { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
