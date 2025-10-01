using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Remote.Linq.Include;

namespace EnterpriseAutomation.Application.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<RolePermissions> _repository;       

        public RolePermissionService(IRepository<RolePermissions> repository)
        {
            _repository = repository;   
        }

        public async Task<IEnumerable<RolePermissions?>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<RolePermissions?>> GetByPermissionId(int permissionId)
        {
            return await _repository.GetWhereAsync(x => x.PermissionId == permissionId);
        }

        public  async Task<IEnumerable<RolePermissions>> GetByRoleId(int roleId)
        {
            return await _repository.GetAll(x => x.RoleId == roleId, null, false, null, null, q => q.Include(rp => rp.Permission));//.GetWhereAsync(x => x.RoleId == roleId);
        }


    }
}
