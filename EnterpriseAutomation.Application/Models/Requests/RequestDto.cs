using EnterpriseAutomation.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.Requests
{
    public class RequestDto
    {
        public int RequestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CurrentStatus { get; set; }
        public int CurrentStep { get; set; }

        public int? UserId { get; set; }
        public string? Username { get; set; }
    }
}
