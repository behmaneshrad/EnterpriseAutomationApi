using EnterpriseAutomation.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers;

[ApiController]
[Route("api/keycloak/roles")]
public class KeycloakRolesController : ControllerBase
{
    private readonly KeycloakService _kc;

    public KeycloakRolesController(KeycloakService kc) => _kc = kc;

    public record RealmRoleCreateRequest(string Name, string? Description);
    public record ClientRoleCreateRequest(string ClientId, string Name, string? Description);

    [HttpPost("realm")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateRealmRole([FromBody] RealmRoleCreateRequest req, CancellationToken ct)
    {
        await _kc.CreateRealmRoleAsync(req.Name, req.Description, ct);
        return NoContent();
    }

}
