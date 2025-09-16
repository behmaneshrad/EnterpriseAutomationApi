using Microsoft.AspNetCore.Mvc;
using EnterpriseAutomation.Infrastructure.Services;
using Newtonsoft.Json;
using EnterpriseAutomation.Application.Externals;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using EnterpriseAutomation.Application.Models.Users;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly KeycloakService _keycloakService;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _context; // Add DbContext dependency

        public AccountController(KeycloakService keycloakService, ILogger<AccountController> logger, AppDbContext context)
        {
            _keycloakService = keycloakService;
            _logger = logger;
            _context = context; // Inject DbContext
        }
        [AllowAnonymous]
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
                    if (await AddUserDataToDB(model)) 
                    {
                        return Ok(new { message = "User created successfully in Keycloak and Database" });
                    }
                    else
                    {
                        return Ok(new { message = "User created successfully in Keycloak but failed to add to Database!" });
                    }
                }

                return BadRequest(new { message = "User creation was not successful in Keycloak" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user in Keycloak");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [AllowAnonymous]
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

        [Authorize(Policy = "Admin")]
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

        [Authorize(Policy = "Admin")]
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

        private async Task<bool> AddUserDataToDB(RegisterDto model)
        {
            try
            {
                // Check if user already exists to avoid duplicates
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (existingUser != null)
                {
                    _logger.LogWarning("User with username {Username} already exists in database", model.Username);
                    return true; // Consider this as success since user exists
                }

                // Create a new user entity with proper field mapping
                var user = new User
                {
                    Username = model.Username,
                    RefreshToken = string.Empty, 
                    Role = 0, 
                    PasswordHash = string.Empty, 
                    
                };

                // Add user to DbContext
                _context.Users.Add(user);

                // Save changes to database
                var result = await _context.SaveChangesAsync();

                // Return true if at least one record was affected
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user data to database for username: {Username}", model.Username);
                return false;
            }
        }
    }
}