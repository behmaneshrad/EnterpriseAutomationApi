using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "نقش های دسترسی")]
public class RolePermissions : BaseEntity
{
    [Key]
    [Display(Name = "آیدی")]
    public int RolePermissionsId { get; set; }

    [Display(Name = "اسم نقش")]
    public int RoleId { get; set; } = default!;

    [Display(Name = "آیدی دسترسی")]
    public int PermissionId { get; set; }

    public virtual Permission Permission { get; set; } = default!;
    public virtual Role Role { get; set; } = default!;
    
}
