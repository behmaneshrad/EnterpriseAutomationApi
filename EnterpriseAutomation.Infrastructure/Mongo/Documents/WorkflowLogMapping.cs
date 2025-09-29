using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Infrastructure.Mongo.Documents;

public static class WorkflowLogMapping
{
    public static WorkflowLogDocument ToDocument(this WorkflowLog src) => new()
    {
        TimestampUtc = DateTime.UtcNow,
        WorkflowId = src.WorkflowId,
        StepId = src.StepId,
        UserId = src.UserId,
        UserName = src.UserName,
        ActionType = src.ActionType,
        Description = src.Description,
        RequestId = src.RequestId,
        PreviousState = src.PreviousState,
        NewState = src.NewState
    };
}
