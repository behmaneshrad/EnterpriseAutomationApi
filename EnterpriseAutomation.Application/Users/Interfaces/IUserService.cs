﻿using System.Threading.Tasks;
using EnterpriseAutomation.Application.Users.Models;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Users.Interfaces
{
    public interface IUserService
    {
        //Task<UserDto?> GetUserByUsernameAsync(string username);

        public Task<IEnumerable<UserDto>> GetAllUserAsync(); //Test method

        Task CreateUserAsync(User user);

        Task<UserDto> GetUserByUserNameAsync(string userName); // ✅ بازگشت به UserDto مطابق UserService فعلی

        //Task<UserDto?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal user);
        //Task<User?> ValidateUserAsync(string username, string password);

    }
}
