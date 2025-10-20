using EnterpriseAutomation.Application.Services.Interfaces;
using System.Security.Claims;

namespace EnterpriseAutomation.Api.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        /// <summary>
        /// دریافت keycloakId از claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string? GetKeyclockId(this ClaimsPrincipal user)
        {
            return user.FindFirst("nameidentifier")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// دریافت UserId از دیتابیس بر اساس KeycloackId
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        public static async Task<int> GetUserIdAsync(this ClaimsPrincipal user,IUserService userService)
        {
            var keycloackId = user.GetKeyclockId();
            if (string.IsNullOrWhiteSpace(keycloackId))
                return 0;

            var dbUser = await userService.GetByKeycloakId(keycloackId);

            return dbUser.UserId;
        }
    }
}
