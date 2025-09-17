using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.Permissions;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permRepo;
        

        public PermissionService(IRepository<Permission> permRepo)
        {
            _permRepo = permRepo;           
        }

        public async Task<IEnumerable<Permission?>> GetAllAsync()
        {
            return await _permRepo.GetAllAsync();
        }



        public async Task<PermissionDto> GetSingleAsync(string PermissionName)
        {

            var permission = _permRepo.GetSingleAsync(x => x.Equals(PermissionName)).Result;

            return new PermissionDto
            {
                Name = permission?.Name ?? "",
                Key = permission?.Key ?? "",
                Description = permission?.Description ?? "",
                IsActive = permission?.IsActive ?? true 
            };
        }

    }
}