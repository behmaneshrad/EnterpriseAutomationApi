using EnterpriseAutomation.Application.Permissions.DTOs;

namespace EnterpriseAutomation.Application.Permissions.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionListItemDto> UpsertAsync(PermissionUpsertDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken ct = default);
        Task<PermissionListItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAllowedRolesByRouteAsync(string routeKey, CancellationToken ct = default);

    }
}
