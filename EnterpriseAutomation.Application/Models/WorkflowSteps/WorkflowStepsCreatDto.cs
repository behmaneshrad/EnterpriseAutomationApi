namespace EnterpriseAutomation.Application.Models.WorkflowSteps
{
    public class WorkflowStepsCreatDto
    {
        public int WorkflowDefinitionId { get; set; } = default!;

        public int Order { get; set; } = default!;

        public string StepName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool Editable { get; set; }
    }
}
