using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Models
{
    public class WorkflowDefinitionDto
    {
        public int WorkflowDefinitionId { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int CreatedById { get; set; }

        public int UpdatedById { get; set;}

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
