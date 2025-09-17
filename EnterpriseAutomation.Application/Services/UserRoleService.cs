using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.Users;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IRepository<UserRole> _userRepository;
        public UserRoleService(IRepository<UserRole> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserRole>> GetByUserId(int userId)
        {
            var users = await _userRepository.GetWhereAsync(x => x.UserId == userId);

            var res = users.Select(a => new UserRole
            {
               UserId = a.UserId,
               RoleId = a.RoleId
            });

            return res;
        }


    }
}
