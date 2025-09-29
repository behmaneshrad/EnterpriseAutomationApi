using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseAutomation.Infrastructure.Mongo.Documents;

public sealed class WorkflowLogDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime TimestampUtc { get; set; }

    public int WorkflowId { get; set; }
    public int StepId { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = "";
    public string ActionType { get; set; } = "";
    public string? Description { get; set; }
    public int RequestId { get; set; }
    public string? PreviousState { get; set; }
    public string? NewState { get; set; }

    // اگر Id عددی اپ را هم می‌خواهی:
    public int AppLogId { get; set; }
}
