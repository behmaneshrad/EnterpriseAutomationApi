namespace EnterpriseAutomation.Application.Permissions.DTOs;

public record PermissionListItemDto(
    Guid Id,
    string Key,
    string? Name,
    string? Description,
    int Version,
    bool IsActive,
    IReadOnlyList<string> Roles
);

public record PermissionUpsertDto(
    string Key,
    string? Name,
    string? Description,
    IReadOnlyList<string> Roles
);
