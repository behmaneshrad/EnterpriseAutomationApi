using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseAutomation.Api.Security;

public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Keycloak داخل توکن claim به نام "authorization" می‌گذارد که شامل permissions است
        // {"permissions":[{"rsname":"requests","scopes":["submit","list"]}, ...]}
        var authzJson = context.User.FindFirst("authorization")?.Value;
        if (!string.IsNullOrEmpty(authzJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(authzJson);
                if (doc.RootElement.TryGetProperty("permissions", out var perms))
                {
                    foreach (var p in perms.EnumerateArray())
                    {
                        var rsname = p.TryGetProperty("rsname", out var rs) ? rs.GetString() : null;
                        if (!string.Equals(rsname, requirement.Resource, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (p.TryGetProperty("scopes", out var scopesEl))
                        {
                            foreach (var s in scopesEl.EnumerateArray())
                            {
                                if (string.Equals(s.GetString(), requirement.Scope, StringComparison.OrdinalIgnoreCase))
                                {
                                    context.Succeed(requirement);
                                    return Task.CompletedTask;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // اگر ساختار claim متفاوت یا خراب بود، صرفاً اجازه نده
            }
        }

        return Task.CompletedTask;
    }
}
