using EnterpriseAutomation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace EnterpriseAutomation.Api.Authorization
{
    public class AutoPermissionRequirement : IAuthorizationRequirement
    {
        public string? CustomPermissionName { get; }
        public bool UseControllerActionFormat { get; }

        // Constructor پیش‌فرض - از فرمت Controller.Action استفاده می‌کند
        public AutoPermissionRequirement()
        {
            UseControllerActionFormat = true;
            CustomPermissionName = null;
        }

        // Constructor برای پرمیشن خاص
        public AutoPermissionRequirement(string permissionName)
        {
            CustomPermissionName = permissionName;
            UseControllerActionFormat = false;
        }
    }

    public class AutoPermissionAuthorizationHandler : AuthorizationHandler<AutoPermissionRequirement>
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IUserService _userService;
        private readonly IRolePermissionService _rolePermissionService;

        public AutoPermissionAuthorizationHandler(
            IUserRoleService userRoleService,
            IUserService userService,
            IRolePermissionService rolePermissionService)
        {
            _userRoleService = userRoleService;
            _userService = userService;
            _rolePermissionService = rolePermissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AutoPermissionRequirement requirement)
        {
            string? permissionName = null;

            if (context.Resource is HttpContext httpContext)
            {
                var endpoint = httpContext.GetEndpoint();
                var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

                if (descriptor != null)
                {
                    permissionName = $"{descriptor.ControllerName.Replace("Controller", "")}.{descriptor.ActionName}";
                }
                else
                {
                    // اگر Metadata پیدا نشد، از DisplayName استفاده می‌کنیم
                    permissionName = endpoint?.DisplayName;
                }
            }

            // اگر هنوز permissionName پیدا نشده باشه → برگرد
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return;
            }

            // بررسی احراز هویت
            if (!(context.User.Identity?.IsAuthenticated ?? false))
            {
                return;
            }

            // گرفتن keycloakId از توکن
            var keycloakId = context.User.FindFirst("nameidentifier")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
            {
                return; // اگر keycloakId معتبر نبود
            }

            var user = await _userService.GetByKeycloakId(keycloakId);
            if (user is null)
            {
                return;
            }

            // گرفتن نقش‌های کاربر
            var userRoles = await _userRoleService.GetByUserId(user.UserId);

            // بررسی دسترسی‌ها
            foreach (var userRole in userRoles)
            {
                var rolePermissions = await _rolePermissionService.GetByRoleId(userRole.RoleId);
                foreach (var rolePermission in rolePermissions)
                {
                    if (rolePermission.Permission?.Name != null &&
                        rolePermission.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
        }
    }
}
