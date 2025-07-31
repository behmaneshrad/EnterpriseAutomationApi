using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public() => Ok("عمومی: بدون توکن");

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private() => Ok("خصوصی: نیاز به توکن");

        [Authorize(Policy = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly() => Ok("فقط ادمین!");

        [Authorize]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest("توکن معتبر نیست.");

            var token = authHeader.Replace("Bearer ", "");
            var roles = JwtRoleExtractor.ExtractRoles(token, "enterprise-api");

            return Ok(roles);
        }
    }
}
