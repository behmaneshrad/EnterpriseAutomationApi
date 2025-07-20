using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseAutomation.Domain.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public ICollection <Request> Requests { get; set; }
    public ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public ICollection<ApprovalStep> ApprovalSteps { get; set; }
}
