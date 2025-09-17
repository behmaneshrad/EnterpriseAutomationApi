using EnterpriseAutomation.Application.Models.Permissions;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IPermissionService
    {

        Task<PermissionDto> GetSingleAsync(string PermissionName);

        Task<IEnumerable<Permission?>> GetAllAsync();

    }
}
