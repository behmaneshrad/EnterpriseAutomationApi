using EnterpriseAutomation.Domain.Entities;
using Microsoft.Extensions.Logging;
using Nest;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Logger.WorkflowLogger
{
    public class WorkflowServiceLogger : IWorkflowServiceLogger
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<WorkflowServiceLogger> _logger;
        private const string indexName = "workflow-logs";

        public WorkflowServiceLogger(ILogger<WorkflowServiceLogger> logger, IElasticClient elasticClient)
        {
            _logger = logger;
            _elasticClient = elasticClient;
        }

        public async Task CreateIndexAsync()
        {
            try
            {
                var createIndexResponse = await _elasticClient.Indices
                    .CreateAsync(indexName, c => c.Map<WorkflowLog>(m =>
                    m.AutoMap()
                    .Properties(p => p.
                    Keyword(k => k.Name(n => n.WorkflowId))
                    .Keyword(k => k.Name(n => n.StepId))
                    .Keyword(k => k.Name(n => n.RequestId))
                    .Keyword(k => k.Name(n => n.UserCreatedId))
                    .Text(k => k.Name(n => n.UserName))
                    .Text(k => k.Name(n => n.Description))
                    .Text(k => k.Name(n => n.NewState))
                    .Text(k => k.Name(n => n.PreviousState))
                    .Keyword(k => k.Name(n => n.ActionType))
                    .Date(k => k.Name(n => n.CreatedAt))
                    )).Settings(s => s.NumberOfShards(1).NumberOfReplicas(0)));

                if (!createIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create index: {DebugInformation}", createIndexResponse.DebugInformation);
                    throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
                }
                _logger.LogInformation("Index {IndexName} created successfully", indexName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index {IndexName}", indexName);
                throw;
            }
        }

        public async Task EnsureIndexExistsAsync()
        {
            if(!await IndexExistsAsync())
            {
                await CreateIndexAsync();
            }
        }

        public async Task<bool> IndexExistsAsync()
        {
            try
            {
                var elasticResponse = await _elasticClient.Indices.ExistsAsync(indexName);
                return elasticResponse.Exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index {IndexName} exists", indexName);
                return false;
            }
        }
    }
}
