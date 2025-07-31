using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;

        //  استخراج نقش‌ها از realm_access.roles
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

        var userIdClaim = principal.FindFirst("user_id")?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int numericUserId))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, numericUserId.ToString()));
        }
        else
        {
        }

        return Task.FromResult(principal);
    }
}
