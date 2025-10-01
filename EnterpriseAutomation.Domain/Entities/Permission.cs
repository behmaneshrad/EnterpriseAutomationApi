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

    [Display(Name = "اسم اکشن")]
    public string? Name { get; set; }

    public virtual ICollection<RolePermissions> RolePermissions { get; set; } = [];
}
