using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "دسترسی ها")]
public class Permission : BaseEntity
{
    [Display(Name = "آیدی")]
    public Guid PermissionId { get; set; }

    [Display(Name = "اسم api")]
    public string Key { get; set; } = default!;

    public string? Name { get; set; }
    public string? Description { get; set; }

    [Display(Name = "ورژن")]
    public int Version { get; set; } = 1;

    [Display(Name = "فعال بودن")]
    public bool IsActive { get; set; } = true;

    public ICollection<PermissionRole> Roles { get; set; } = new List<PermissionRole>();
}
