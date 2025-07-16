namespace EnterpriseAutomation.Domain.Entities;

public class User
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public ICollection<Request> Requests { get; set; }
}
