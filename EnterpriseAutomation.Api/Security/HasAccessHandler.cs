using System.Security.Claims;
using System.Text.RegularExpressions;
using EnterpriseAutomation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseAutomation.Api.Security
{
    /// <summary>
    /// پالیسی داینامیک HasAccess:
    /// اگر هرکدام از نقش‌های کاربر (از JWT) با نقش‌های مجازِ تعریف‌شده در DB
    /// برای routeKey (path|method) یا controllerKey (controller:{name}|method) همپوشانی داشته باشد، دسترسی داده می‌شود.
    /// در غیر این صورت، authorization موفق نمی‌شود و 403 خواهید دید.
    /// </summary>
    public sealed class HasAccessHandler : AuthorizationHandler<HasAccessRequirement>
    {
        private readonly IPermissionService _perm;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<HasAccessHandler> _logger;
        private readonly bool _emitDebugHeaders;

        public HasAccessHandler(
            IPermissionService perm,
            IHttpContextAccessor http,
            ILogger<HasAccessHandler> logger,
            IConfiguration config)
        {
            _perm = perm;
            _http = http;
            _logger = logger;

            // اگر نخواستی هدرهای دیباگ ارسال شوند، این کلید را در appsettings=false کن
            _emitDebugHeaders = config.GetValue("Authorization:EnableAuthDebugHeaders", false);
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasAccessRequirement requirement)
        {
            // 1) احراز هویت بررسی شود
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("[HasAccess] User not authenticated.");
                return;
            }

            var http = _http.HttpContext;
            if (http is null)
            {
                _logger.LogWarning("[HasAccess] HttpContext is null.");
                return;
            }

            // 2) ساخت کلیدها
            string method = http.Request.Method ?? "GET";
            string rawPath = http.Request.Path.Value ?? "/";

            string routeKey = Normalize($"{rawPath}|{method}");

            var cad = http.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            string? controllerName = cad?.ControllerName;
            string? controllerKey = controllerName is null ? null :
                                    Normalize($"controller:{controllerName}|{method}");

            // 3) نقش‌های کاربر از JWT (ClaimTypes.Role را از Program.cs اضافه کرده‌ایم)
            var userRoles = context.User.FindAll(ClaimTypes.Role)
                                        .Select(c => c.Value)
                                        .Where(v => !string.IsNullOrWhiteSpace(v))
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 4) نقش‌های مجاز از DB
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var byRoute = await _perm.GetAllowedRolesByRouteAsync(routeKey, http.RequestAborted);
            foreach (var r in byRoute) allowed.Add(r);

            if (!string.IsNullOrWhiteSpace(controllerKey))
            {
                var byController = await _perm.GetAllowedRolesByRouteAsync(controllerKey!, http.RequestAborted);
                foreach (var r in byController) allowed.Add(r);
            }

            // 5) لاگ و هدرهای دیباگ (اختیاری)
            _logger.LogInformation(
                "[HasAccess] Method={Method} Path={Path} routeKey={RouteKey} controllerKey={ControllerKey} UserRoles=[{UserRoles}] Allowed=[{Allowed}]",
                method, rawPath, routeKey, controllerKey ?? "(null)",
                string.Join(",", userRoles), string.Join(",", allowed));

            if (_emitDebugHeaders && !http.Response.HasStarted)
            {
                TryAddHeader(http.Response, "X-Auth-Method", method);
                TryAddHeader(http.Response, "X-Auth-Path", rawPath);
                TryAddHeader(http.Response, "X-Auth-RouteKey", routeKey);
                if (!string.IsNullOrWhiteSpace(controllerKey))
                    TryAddHeader(http.Response, "X-Auth-ControllerKey", controllerKey!);
                TryAddHeader(http.Response, "X-Auth-UserRoles", string.Join(",", userRoles));
                TryAddHeader(http.Response, "X-Auth-AllowedRoles", string.Join(",", allowed));
            }

            // 6) تصمیم
            if (allowed.Count == 0)
            {
                // چیزی برای این کلید در DB تعریف نشده ⇒ اجازه نمی‌دهیم (امن‌تر)
                _logger.LogWarning("[HasAccess] No permission defined for keys. Access denied.");
                return;
            }

            if (userRoles.Overlaps(allowed))
            {
                _logger.LogInformation("[HasAccess] Access granted.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("[HasAccess] Roles mismatch. Access denied.");
            }
        }

        // نرمال‌سازی: trim، حذف اسلش پایانی، تبدیل به lowercase، حذف // اضافی
        private static string Normalize(string s)
        {
            s = (s ?? string.Empty).Trim();
            s = Regex.Replace(s, "/{2,}", "/");
            if (s.EndsWith("/") && s.Length > 1) s = s.TrimEnd('/');
            return s.ToLowerInvariant();
        }

        private static void TryAddHeader(HttpResponse response, string key, string? value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                    response.Headers[key] = value.Length > 512 ? value[..512] + "..." : value;
            }
            catch
            {
                // ignore header failures (e.g., after response started)
            }
        }
    }
}
