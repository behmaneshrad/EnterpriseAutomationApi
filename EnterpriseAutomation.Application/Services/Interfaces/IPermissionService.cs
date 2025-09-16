using EnterpriseAutomation.Application.Models.Permissions;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionListItemDto> UpsertAsync(PermissionUpsertDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken ct = default);
        Task<PermissionListItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAllowedRolesByRouteAsync(string routeKey, CancellationToken ct = default);

    }
}
