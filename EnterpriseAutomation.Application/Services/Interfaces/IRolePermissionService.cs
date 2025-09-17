using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IRolePermissionService
    {
        Task<IEnumerable<RolePermissions?>> GetAllAsync();

        Task<IEnumerable<RolePermissions?>> GetByPermissionId(int permissionId);

        Task<IEnumerable<RolePermissions?>> GetByRoleId(int roleId);
    }
}
