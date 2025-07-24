using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EnterpriseAutomation.Domain.Entities.Base;

namespace EnterpriseAutomation.Domain.Entities;

[Display(Name = "موجودیت کاربران")]
public class User : BaseEntity
{
    [Key]
    public int UserId { get; set; } 
    public string Username { get; set; } = string.Empty; 
    public string RefreshToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; 
    public string PasswordHash { get; set; } = string.Empty;

    public virtual ICollection<Request> Requests { get; set; }
    public virtual ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public virtual ICollection<ApprovalStep> ApprovalSteps { get; set; }
}
