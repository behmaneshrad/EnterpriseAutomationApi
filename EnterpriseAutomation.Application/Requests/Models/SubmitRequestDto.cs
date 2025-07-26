using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Requests.Models
{
    public class SubmitRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
    }
}
