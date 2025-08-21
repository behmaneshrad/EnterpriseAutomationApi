namespace EnterpriseAutomation.Application.Users.Models
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public required int Role { get; set; }
    }
}

