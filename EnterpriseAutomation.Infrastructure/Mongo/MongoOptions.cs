namespace EnterpriseAutomation.Infrastructure.Mongo;

public sealed class MongoOptions
{
    public string ConnectionString { get; init; } = null!;
    public string Database { get; init; } = null!;
    public string WorkflowLogCollection { get; init; } = null!;
    public bool EnsureIndexes { get; init; }
}
