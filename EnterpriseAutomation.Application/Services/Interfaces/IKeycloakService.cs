using EnterpriseAutomation.Application.Models;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IKeycloakService
    {
        Task<KeycloakResponse> LoginAsync(LoginRequest request);
        Task<KeycloakResponse> RegisterAsync(RegisterRequest request);
    }
}
