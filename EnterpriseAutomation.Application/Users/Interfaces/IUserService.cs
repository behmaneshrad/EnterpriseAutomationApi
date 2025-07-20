using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Dtos;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Users.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByUsernameAsync(string username);

        Task CreateUserAsync(User user);

        Task<UserDto?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal user);
        Task<User?> ValidateUserAsync(string username, string password);

    }
}