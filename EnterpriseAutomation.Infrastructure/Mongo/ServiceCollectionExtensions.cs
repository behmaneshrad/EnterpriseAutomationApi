using EnterpriseAutomation.Application.Logging;
using EnterpriseAutomation.Infrastructure.Mongo.Documents;
using EnterpriseAutomation.Infrastructure.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnterpriseAutomation.Infrastructure.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoWorkflowLogging(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<MongoOptions>().Bind(cfg.GetSection("Mongo"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            return new MongoClient(opt.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var db = client.GetDatabase(opt.Database);
            var col = db.GetCollection<WorkflowLogDocument>(opt.WorkflowLogCollection);

            if (opt.EnsureIndexes)
            {
                var keys = Builders<WorkflowLogDocument>.IndexKeys
                    .Descending(x => x.TimestampUtc)
                    .Ascending(x => x.WorkflowId)
                    .Ascending(x => x.RequestId)
                    .Ascending(x => x.StepId);
                col.Indexes.CreateOne(new CreateIndexModel<WorkflowLogDocument>(keys));
            }
            return col;
        });

        services.AddScoped<IWorkflowLogWriter, MongoWorkflowLogWriter>();
        return services;
    }
}
