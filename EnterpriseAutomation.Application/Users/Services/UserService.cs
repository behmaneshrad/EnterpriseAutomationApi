using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Dtos;
using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnterpriseAutomation.Application.Users.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            return new UserDto
            {
                Username = user.Username,
                Role = user.Role
                // Add Email if your User entity has an Email property
            };
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal userClaims)
        {
            var username = userClaims.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            return new UserDto
            {
                Username = user.Username,
                Role = user.Role
                // Add Email if your User entity has an Email property
            };
        }
    }
}