using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<RolePermissions> _repository;       

        public RolePermissionService(IRepository<RolePermissions> repository)
        {
            _repository = repository;   
        }


    }
}
