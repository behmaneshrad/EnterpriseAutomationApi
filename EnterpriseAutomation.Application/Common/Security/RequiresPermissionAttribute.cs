using Microsoft.AspNetCore.Authorization;

namespace EnterpriseAutomation.Api.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequiresPermissionAttribute : AuthorizeAttribute
{
    public RequiresPermissionAttribute(string resource, string scope)
    {
        Policy = PermissionPolicyName(resource, scope);
    }

    public static string PermissionPolicyName(string resource, string scope)
        => $"perm:{resource}#{scope}";
}
