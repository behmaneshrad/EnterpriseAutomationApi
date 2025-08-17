using Microsoft.AspNetCore.Authorization;

namespace EnterpriseAutomation.Api.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PolicyKey { get; }

    public PermissionRequirement(string policyKey)
    {
        PolicyKey = policyKey;
    }
}
