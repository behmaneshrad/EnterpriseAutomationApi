namespace EnterpriseAutomation.Infrastructure.Options;

public class KeycloakOptions
{
    public string Authority { get; set; } = default!; 
    public string Realm { get; set; } = default!;     

    // برای ولیدیشن JWT
    public string Audience { get; set; } = default!;  

    
    public string AdminClientId { get; set; } = default!;
    public string AdminClientSecret { get; set; } = default!;
}
