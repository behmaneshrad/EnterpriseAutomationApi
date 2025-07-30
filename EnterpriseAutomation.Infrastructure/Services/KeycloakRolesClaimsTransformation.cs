using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;
        var realmAccessClaim = principal.FindFirst("realm_access");

        if (realmAccessClaim != null)
        {
            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
            if (doc.RootElement.TryGetProperty("roles", out var roles))
            {
                foreach (var role in roles.EnumerateArray())
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
