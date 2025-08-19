namespace EnterpriseAutomation.Application.Permissions.DTOs;

public record PermissionListItemDto(
    int Id,
    string Key,
    string? Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<int> RoleIds
);

public record PermissionUpsertDto(
    string Key,
    string? Name,
    string? Description,
    IReadOnlyList<int> RoleIds
);
