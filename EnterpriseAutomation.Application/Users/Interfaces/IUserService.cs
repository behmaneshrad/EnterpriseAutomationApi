using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Models;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Users.Interfaces
{
    public interface IUserService
    {
        //Task<UserDto?> GetUserByUsernameAsync(string username);

        Task CreateUserAsync(User user);

        public Task<UserDto> GetUserByUserNameAsync(string userName);

        //Task<UserDto?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal user);
        //Task<User?> ValidateUserAsync(string username, string password);

    }
}