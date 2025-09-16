using EnterpriseAutomation.Application.UserRole.Models;

namespace EnterpriseAutomation.Application.Users.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<UserRoleDto> UserRoles { get; set; } = [];
    }
}

