using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.Permissions
{
    public class PermissionDto
    {       
        public int PermissionId { get; set; } 
        public string Key { get; set; } = string.Empty;      
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
