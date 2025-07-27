using Microsoft.AspNetCore.Mvc;
using EnterpriseAutomation.Application.Users.Models;
using EnterpriseAutomation.Infrastructure.Services;
using Newtonsoft.Json;
using EnterpriseAutomation.Application.Externals;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly KeycloakService _keycloakService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(KeycloakService keycloakService, ILogger<AccountController> logger)
        {
            _keycloakService = keycloakService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var created = await _keycloakService.CreateUserAsync(
                    model.Username,
                    model.Email,
                    model.Password,
                    model.FirstName ?? "",
                    model.LastName ?? ""
                );

                if (created)
                {
                    return Ok(new { message = "User created successfully in Keycloak" });
                }

                return BadRequest(new { message = "User creation was not successful in Keycloak" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in Keycloak");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var responseContent = await _keycloakService.LoginUserAsync(model.Username, model.Password);
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(responseContent);

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return BadRequest(new { message = "Login failed", details = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _keycloakService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Failed to retrieve users", details = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _keycloakService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { message = "Failed to retrieve roles", details = ex.Message });
            }
        }
    }
}