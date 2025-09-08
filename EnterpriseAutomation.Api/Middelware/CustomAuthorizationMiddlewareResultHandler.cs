using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace EnterpriseAutomation.Api.Middelware
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // اگر احراز هویت نشده باشد
            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "لطفا ابتدا وارد سیستم شوید",
                    statusCode = 401
                });
                return;
            }
            // اگر دسترسی نداشته باشد
            else if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "شما دسترسی لازم برای این عملیات را ندارید",
                    statusCode = 403
                });
                return;
            }

            // در صورت موفقیت، اجازه ادامه کار
            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
