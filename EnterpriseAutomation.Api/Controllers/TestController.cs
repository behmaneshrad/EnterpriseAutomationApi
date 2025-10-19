using Application.Common.Security;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Services;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Security.Claims;

namespace Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly ITestServiceMeet8 _ts;

        public TestController(ILogger<TestController> logger, ITestServiceMeet8 ts,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _ts = ts;

        }

        [HttpGet("CurrentUser")]
        public IActionResult GetCurrentUser()
        {
            var claimp = ClaimsPrincipal.Claims.ToDictionary<string, string?>(static c => c.Type, c => c.Value);
            if (User.Identity.IsAuthenticated)
            {
                var keycloakId = User.FindFirst("Keycloak_id")?.Value;
                if (keycloakId == null)
                {
                    return NotFound();
                }

                return Ok(keycloakId);
            }
            var keycloakId2 = User.FindFirst("sub")?.Value;
            return Ok(keycloakId2 == string.Empty ? $"{keycloakId2}" : "is null");
        }


        [AllowAnonymous]
        [HttpPost("cors")]
        public IActionResult Post([FromBody] object body) => Ok(new { msg = "CORS OK (POST)", body });

        [AllowAnonymous]
        [HttpGet("get")]
        public IActionResult Get()
        {
            _logger.LogInformation("Test log from TestController at {time}", DateTime.UtcNow);
            return Ok("check elastic");
        }

        [AllowAnonymous]
        [HttpGet("ITestServiceMeet8")]
        public IActionResult Get(int id)
        {
            var res = _ts.Get(id);
            return Ok(res);
        }



        //[HttpGet("public")]
        //public IActionResult Public() => Ok("عمومی: بدون توکن");

        //[Authorize]
        //[HttpGet("private")]
        //public IActionResult Private() => Ok("خصوصی: نیاز به توکن");

        //[Authorize(Policy = "Admin")]
        //[HttpGet("admin-only")]
        //public IActionResult AdminOnly() => Ok("فقط ادمین!");

        //[Authorize]
        //[HttpGet("roles")]
        //public IActionResult GetRoles()
        //{
        //    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        //    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        //        return BadRequest("توکن معتبر نیست.");

        //    var token = authHeader.Replace("Bearer ", "");
        //    var roles = JwtRoleExtractor.ExtractRoles(token, "enterprise-api");

        //    return Ok(roles);
        //}
    }
    class userTestDto
    {
        public string? userID { get; set; }
    }
}
