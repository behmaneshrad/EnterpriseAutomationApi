using EnterpriseAutomation.Application.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers;

[ApiController]
[Route("api/keycloak/roles")]
public class KeycloakRolesController : ControllerBase
{
    private readonly IKeycloakService _kc;

    public KeycloakRolesController(IKeycloakService kc) => _kc = kc;

    public record RealmRoleCreateRequest(string Name, string? Description);
    public record ClientRoleCreateRequest(string ClientId, string Name, string? Description);  

}
