using EnterpriseAutomation.Application.Logging;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Mongo.Documents;
using MongoDB.Driver;

namespace EnterpriseAutomation.Infrastructure.Mongo.Repositories;

internal sealed class MongoWorkflowLogWriter : IWorkflowLogWriter
{
    private readonly IMongoCollection<WorkflowLogDocument> _col;
    public MongoWorkflowLogWriter(IMongoCollection<WorkflowLogDocument> col) => _col = col;

    public async Task WriteAsync(WorkflowLog log, CancellationToken ct = default)
    {
        var doc = log.ToDocument();
        await _col.InsertOneAsync(doc, cancellationToken: ct);
        
    }
}
