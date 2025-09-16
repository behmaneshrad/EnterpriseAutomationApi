using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Permissions.Interfaces;
using EnterpriseAutomation.Application.Permissions.Models;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Application.Permissions.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permRepo;
        private readonly IRepository<Role> _roleRepo; // فرض می‌کنیم جدول Role وجود دارد       

        public PermissionService(IRepository<Permission> permRepo, IRepository<Role> roleRepo)
        {
            _permRepo = permRepo;
            _roleRepo = roleRepo;
            
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









































        public async Task<PermissionListItemDto> UpsertAsync(PermissionUpsertDto dto, CancellationToken ct = default)
        {
            // invalidate active versions
            var actives = await _permRepo.GetWhereAsync(p => p.Key == dto.Key && p.IsActive);
            foreach (var p in actives)
            {
                if (p is null) continue;
                p.IsActive = false;
                p.UpdatedAt = DateTime.UtcNow;
                _permRepo.UpdateEntity(p);
            }

            var entity = new Permission
            {
                Key = dto.Key.Trim(),
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };

            foreach (var roleId in dto.RoleIds)
            {
                entity.Roles.Add(new RolePermissions { RoleId = roleId, PermissionId = entity.PermissionId });
            }

            await _permRepo.InsertAsync(entity);
            await _permRepo.SaveChangesAsync();

            return await MapAsync(entity.PermissionId, ct) ?? throw new InvalidOperationException();
        }

        public async Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var query = _permRepo.GetQueryable(q => q.Include(p => p.Roles).ThenInclude(r => r.Role), asNoTracking: true);
            var list = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

            return list.Select(p => new PermissionListItemDto(
                p.PermissionId,
                p.Key,
                p.Name,
                p.Description,
                p.IsActive,
                p.Roles.Select(r => r.RoleId).ToList()
            )).ToList();
        }

        public Task<PermissionListItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => MapAsync(id, ct);

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _permRepo.GetFirstOrDefaultAsync(p => p.PermissionId == id);
            if (entity is null) return false;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _permRepo.UpdateEntity(entity);
            await _permRepo.SaveChangesAsync();
            return true;
        }

        private async Task<PermissionListItemDto?> MapAsync(int id, CancellationToken ct)
        {
            var p = await _permRepo.GetFirstWithInclude(
                x => x.PermissionId == id,
                q => q.Include(x => x.Roles).ThenInclude(r => r.Role),
                asNoTracking: true);

            if (p is null) return null;

            return new PermissionListItemDto(
                p.PermissionId,
                p.Key,
                p.Name,
                p.Description,
                p.IsActive,
                p.Roles.Select(r => r.RoleId).ToList()
            );
        }

        public async Task<IReadOnlyList<string>> GetAllowedRolesByRouteAsync(string routeKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(routeKey))
                return Array.Empty<string>();

            routeKey = routeKey.Trim().TrimEnd('/').ToLowerInvariant();

            var q = _permRepo.GetQueryable(q => q.Include(p => p.Roles).ThenInclude(r => r.Role), asNoTracking: true);
            var p = await q.Where(x => x.Key.ToLower() == routeKey && x.IsActive)
                           .FirstOrDefaultAsync(ct);

            if (p is null || p.Roles.Count == 0) return Array.Empty<string>();

            return p.Roles.Where(r => r.Role != null)
                          .Select(r => r.Role.RoleName)
                          .Where(r => !string.IsNullOrWhiteSpace(r))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();
        }
    }
}