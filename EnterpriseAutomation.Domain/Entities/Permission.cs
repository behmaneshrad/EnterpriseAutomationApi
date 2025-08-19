using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "دسترسی ها")]
public class Permission : BaseEntity
{
    [Key]
    [Display(Name = "آیدی")]
    public int PermissionId { get; set; }

    [Display(Name = "اسم api")]
    public string Key { get; set; } = default!;

    [Display(Name = "اسم اکشن")]
    public string? Name { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "فعال بودن")]
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermissions> Roles { get; set; } = [];
}
