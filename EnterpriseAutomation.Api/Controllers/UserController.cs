using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Dtos;
using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userDto = await _userService.GetCurrentUserAsync(User);
            if (userDto == null)
                return Unauthorized();

            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var passwordHasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHasher.HashPassword(null, dto.Password) ,
                Role = "user"
            };

            await _userService.CreateUserAsync(user);
            var userDto = new UserDto { Username = user.Username, Role = user.Role };

            return CreatedAtAction(nameof(GetCurrentUser), new { username = user.Username }, userDto);
        }
    }
}
