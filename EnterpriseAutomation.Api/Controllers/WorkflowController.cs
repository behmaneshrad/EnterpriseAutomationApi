using EnterpriseAutomation.Application.Logging;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowLogWriter _logWriter;
    public WorkflowController(IWorkflowLogWriter logWriter) => _logWriter = logWriter;

    [HttpPost("{workflowId:int}/steps/{stepId:int}/approve")]
    public async Task<IActionResult> Approve(int workflowId, int stepId, [FromBody] string? description, CancellationToken ct)
    {
        var log = new WorkflowLog
        {   
            WorkflowId = workflowId,
            StepId = stepId,
            UserId = null,                
            UserName = User?.Identity?.Name ?? "system",
            ActionType = "Approve",
            Description = description,
            RequestId = 1234,
            PreviousState = "Pending",
            NewState = "Approved"
        };

        await _logWriter.WriteAsync(log, ct);
        return Ok(new { ok = true });
    }
}
