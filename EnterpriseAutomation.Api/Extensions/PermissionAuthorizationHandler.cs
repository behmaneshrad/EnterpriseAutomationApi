using EnterpriseAutomation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EnterpriseAutomation.Api.Extensions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserService _userService;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IUserService userService, IRolePermissionService rolePermissionService, IPermissionService permissionService)
        {
            _userService = userService;
            _rolePermissionService = rolePermissionService;
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Fail();
                return;
            }
            if (!int.TryParse(userIdClaim, out int userId))
            {
                context.Fail();
                return;
            }
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || user.UserRoles == null)
            {
                context.Fail();
                return;
            }
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            var permission = await _permissionService.GetSingleAsync(p => p.Name == requirement.PermissionName);
            if (permission == null)
            {
                context.Fail();
                return;
            }
            var allowedRoleIds = (await _rolePermissionService.GetAllAsync(rp => rp.PermissionId == permission.PermissionId)).Select(rp => rp.RoleId);
            if (userRoleIds.Intersect(allowedRoleIds).Any())
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }
        public PermissionRequirement(string permissionName)
        {
            PermissionName = permissionName;
        }
    }
}
