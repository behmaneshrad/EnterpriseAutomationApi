using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Application;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Application.Models.Users;

namespace EnterpriseAutomation.WebApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IKeycloakService _keycloakService;

        public UserController(IUserService userService, IKeycloakService keycloakService)
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
    }
}
