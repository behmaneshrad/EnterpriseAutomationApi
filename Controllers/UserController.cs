using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Models;
using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Application;
using EnterpriseAutomation.Infrastructure.Services;

namespace EnterpriseAutomation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly KeycloakService _keycloakService;

        public UserController(IUserService userService, KeycloakService keycloakService)
        {
            _userService = userService;
            _keycloakService = keycloakService;
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            //var userDto = await _userService.GetCurrentUserAsync(User);
            //if (userDto == null)
            //    return Unauthorized();
            return Ok("userDto");
        }

        [AllowAnonymous]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateUser([FromBody] UserRegisterDto dto)
        {
            //var userDto = await _userService.ValidateUserAsync(dto.Username, dto.Password);

            //if (userDto == null)
            //    return Unauthorized("Invalid credentials");

            return Ok("userDto");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            //var created = await _keycloakService.CreateUserAsync(dto.Username, dto.Email, dto.Password);
            //if (!created)
            //    return BadRequest("Failed to create user in Keycloak");

            var passwordHasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHasher.HashPassword(null, dto.Password),
                Role = "user"
            };

            await _userService.CreateUserAsync(user);
            var userDto = new UserDto { Username = user.Username, Role = user.Role };

            return CreatedAtAction(nameof(GetCurrentUser), new { username = user.Username }, userDto);
        }
        /// Gets users directly from Keycloak
        [Authorize(Policy = "Admin")]
        [HttpGet("keycloak-users")]
        public async Task<IActionResult> GetKeycloakUsers()
        {
            var result = await _keycloakService.GetUsersAsync();
            return Ok(result);
        }

        /// Gets roles directly from Keycloak
        [Authorize(Policy = "Admin")]
        [HttpGet("keycloak-roles")]
        [Authorize]
        public async Task<IActionResult> GetKeycloakRoles()
        {
            var result = await _keycloakService.GetRolesAsync();
            return Ok(result);
        }
        ///Check the role of the logged-in user and send data related to the access level
        [HttpGet("access-level")]
        public IActionResult GetAccessLevelData()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // User not logged in, return limited access data or anonymous info
                return Ok(new { AccessLevel = "Anonymous", Data = "Limited access data" });
            }

            // Get user's roles from claims
            var roles = User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

            if (roles.Contains("Admin"))
            {
                return Ok(new { AccessLevel = "Admin", Data = "Full admin data" });
            }
            else if (roles.Contains("user"))
            {
                return Ok(new { AccessLevel = "User", Data = "Regular user data" });
            }
            else
            {
                return Ok(new { AccessLevel = "Unknown", Data = "Restricted data" });
            }
        }
        /// end point for pagination and search
        [HttpGet("search")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination values");

            var result = await _userService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
            return Ok(result);
        }

        }
}
