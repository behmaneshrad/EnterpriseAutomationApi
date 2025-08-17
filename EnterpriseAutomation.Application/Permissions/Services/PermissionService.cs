using EnterpriseAutomation.Application.Permissions.DTOs;
using EnterpriseAutomation.Application.Permissions.Interfaces;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Application.Permissions.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permRepo;

        public PermissionService(IRepository<Permission> permRepo)
        {
            _permRepo = permRepo;
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

            var lastVersion = 0;
            var allForKey = await _permRepo.GetWhereAsync(p => p.Key == dto.Key);
            foreach (var p in allForKey)
                if (p is not null && p.Version > lastVersion) lastVersion = p.Version;

            var entity = new Permission
            {
                Key = dto.Key.Trim(),
                Name = dto.Name,
                Description = dto.Description,
                Version = lastVersion + 1,
                IsActive = true
            };

            foreach (var role in dto.Roles ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(role)) continue;
                entity.Roles.Add(new PermissionRole { RoleName = role.Trim() });
            }

            await _permRepo.InsertAsync(entity);
            await _permRepo.SaveChangesAsync();

            return await MapAsync(entity.PermissionId, ct) ?? throw new InvalidOperationException();
        }

        public async Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var query = _permRepo.GetQueryable(q => q.Include(p => p.Roles), asNoTracking: true);
            var list = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

            return list.Select(p => new PermissionListItemDto(
                p.PermissionId, p.Key, p.Name, p.Description, p.Version, p.IsActive,
                p.Roles.Select(r => r.RoleName).ToList()
            )).ToList();
        }

        public Task<PermissionListItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => MapAsync(id, ct);

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _permRepo.GetFirstOrDefaultAsync(p => p.PermissionId == id);
            if (entity is null) return false;
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            _permRepo.UpdateEntity(entity);
            await _permRepo.SaveChangesAsync();
            return true;
        }

        private async Task<PermissionListItemDto?> MapAsync(Guid id, CancellationToken ct)
        {
            var p = await _permRepo.GetFirstWithInclude(
                x => x.PermissionId == id,
                q => q.Include(x => x.Roles),
                asNoTracking: true);

            if (p is null) return null;

            return new PermissionListItemDto(
                p.PermissionId, p.Key, p.Name, p.Description, p.Version, p.IsActive,
                p.Roles.Select(r => r.RoleName).ToList()
            );
        }
        // کلیدها همگی باید lowercase و بدون اسلش انتهایی باشند.
        private static readonly Dictionary<string, string[]> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            
            ["/api/keycloak/roles/realm|post"] = new[] { "admin", "approver" },

            // مثال‌های دیگر:
            // ["/api/requests|get"] = new[] { "admin", "user" },
            // ["/api/users|delete"] = new[] { "admin" },
        };

        public async Task<IReadOnlyList<string>> GetAllowedRolesByRouteAsync(string routeKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(routeKey))
                return Array.Empty<string>();

            routeKey = routeKey.Trim().TrimEnd('/').ToLowerInvariant();

            var q = _permRepo.GetQueryable(q => q.Include(p => p.Roles), asNoTracking: true);
            var p = await q.Where(x => x.Key.ToLower() == routeKey && x.IsActive)
                           .OrderByDescending(x => x.Version)
                           .FirstOrDefaultAsync(ct);

            if (p is null || p.Roles.Count == 0) return Array.Empty<string>();

            return p.Roles.Select(r => r.RoleName)
                          .Where(r => !string.IsNullOrWhiteSpace(r))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();
        }

    }
}
