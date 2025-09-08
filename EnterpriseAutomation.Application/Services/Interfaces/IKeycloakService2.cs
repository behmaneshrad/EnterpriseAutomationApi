using EnterpriseAutomation.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IKeycloakService2
    {
        Task<KeycloakResponse> LoginAsync(LoginRequest request);
        Task<KeycloakResponse> RegisterAsync(RegisterRequest request);
    }
}
