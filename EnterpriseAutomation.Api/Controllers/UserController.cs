using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;            //ّfor hashing password
//using System.Security.Cryptography;            //ّfor  sh256 hashing  password
//using System.Text;                              //for hashing sh256
//using BCrypt.Net;                                 //for bycript hasshing  password  
using System.Threading.Tasks;                  // For async Task
using EnterpriseAutomation.Application.Users.Dtos;  // For UserRegisterDto, UserDto
using EnterpriseAutomation.Application.Users.Interfaces;  // For IUserService
using EnterpriseAutomation.Domain.Entities;    // For User entity
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor; // To get JWT claims

    public UserController(IUserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    // GET: api/user/current
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        // Extract username from JWT claims
        var username = _httpContextAccessor.HttpContext.User.FindFirst("preferred_username")?.Value;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        // Hash password 
        var hashedPassword = HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = hashedPassword,
            //Email = dto.Email
        };

        await _userService.CreateUserAsync(user);

        return CreatedAtAction(nameof(GetCurrentUser), new { username = user.Username }, user);
    }

    private string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        var hashedPassword = passwordHasher.HashPassword(null, password);
        return hashedPassword;
    }
    //private string HashPasswordWithSHA256(string password)
    //{
    //using (SHA256 sha256 = SHA256.Create())
    //{
    //byte[] bytes = Encoding.UTF8.GetBytes(password);
    //byte[] hashBytes = sha256.ComputeHash(bytes);
    //return Convert.ToBase64String(hashBytes);
    // }
    //}


    //private string HashPasswordWithBCrypt(string password)
    //{
        //return BCrypt.Net.BCrypt.HashPassword(password);
    //}
    
    
}