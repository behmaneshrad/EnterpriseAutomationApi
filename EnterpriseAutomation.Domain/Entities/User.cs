using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EnterpriseAutomation.Domain.Entities.Base;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "کاربران")]
public class User : BaseEntity
{
    [Key]
    [Display(Name ="آی دی کاربر")]
    public int UserId { get; set; }

    [Display(Name = "یوزرنیم")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "رفرش توکن")]
    public string RefreshToken { get; set; } = string.Empty;

    [Display(Name = "نقش")]
    public int Role { get; set; }

    [Display(Name = "پسورد هش")]
    public string PasswordHash { get; set; } = string.Empty;

    [Display(Name = "شناسه کی‌کلاک")]
    public string? KeycloakId { get; set; }

    public virtual ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; } = [];
    public virtual ICollection<Request> Requests { get; set; } = [];
    public virtual ICollection<ApprovalStep> ApprovalSteps { get; set; } = [];

    public virtual ICollection<Role> Roles { get; set; } = [];

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}
