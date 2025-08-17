using EnterpriseAutomation.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "نقش های دسترسی")]
public class PermissionRole : BaseEntity
{
    [Display(Name = "آیدی")]
    public int PermissionRoleId { get; set; }

    [Display(Name = "آیدی دسترسی")]
    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = default!;

    [Display(Name = "اسم نقش")]
    public string RoleName { get; set; } = default!;
}
