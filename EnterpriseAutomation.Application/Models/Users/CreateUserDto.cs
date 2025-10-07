using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Models.Users
{
    public class CreateUserDto
    {

        public string Username { get; set; } = string.Empty;


        public string RefreshToken { get; set; } = string.Empty;


        public int Role { get; set; }


        public string PasswordHash { get; set; } = string.Empty;

        public string? KeycloakId { get; set; }

        public Guid ExternalGuid { get; set; }
    }
}
