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
    [AllowAnonymous]
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
                Role = 0
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
    }
}
