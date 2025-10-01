using System.Security.Claims;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Api.Security;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly AppDbContext _db;

    public PermissionHandler(AppDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // اگر احراز هویت نشده
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }

        // نقش‌های کاربر از توکن (realm_access + resource_access تبدیل به ClaimTypes.Role در Program.cs)
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (userRoles.Count == 0)
        {
            context.Fail();
            return;
        }

        // Permission فعّال مربوط به این PolicyKey
        var permission = await _db.Permissions
            .Include(p => p.Roles)
            .Where(p => p.Name == requirement.PolicyKey)
            .FirstOrDefaultAsync();

        if (permission is null || permission.Roles.Count == 0)
        {
            // اگر Policy تعریف نشده است، به‌صورت پیش‌فرض رد می‌کنیم (امن‌تر است)
            context.Fail();
            return;
        }

        var allowed = permission.Roles.Select(r => r.Role.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        bool any = userRoles.Overlaps(allowed);

        if (any) context.Succeed(requirement);
        else context.Fail();
    }
}
