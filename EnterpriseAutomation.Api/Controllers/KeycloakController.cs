using EnterpriseAutomation.Application.Models;
using EnterpriseAutomation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeycloakController : ControllerBase
    {
        private readonly IKeycloakService _keycloakService;

        public KeycloakController(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<KeycloakResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _keycloakService.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<KeycloakResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _keycloakService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
