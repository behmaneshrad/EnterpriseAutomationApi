using Microsoft.AspNetCore.Authorization;

namespace EnterpriseAutomation.Api.Security;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public string Scope { get; }
    public PermissionRequirement(string resource, string scope)
    {
        this.Resource = resource;
        this.Scope = scope;
    }
}
