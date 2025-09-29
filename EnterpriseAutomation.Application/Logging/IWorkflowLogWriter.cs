using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Logging;

public interface IWorkflowLogWriter
{
    Task WriteAsync(WorkflowLog log, CancellationToken ct = default);
}
