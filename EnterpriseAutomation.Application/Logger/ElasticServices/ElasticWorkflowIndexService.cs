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

namespace EnterpriseAutomation.Application.Logger.ElasticServices
{
    public class ElasticWorkflowIndexService : IElasticWorkflowIndexService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticWorkflowIndexService> _logger;
        private const string indexName = "workflow-logs";

        public ElasticWorkflowIndexService(ILogger<ElasticWorkflowIndexService> logger, IElasticClient elasticClient)
        {
            _logger = logger;
            _elasticClient = elasticClient;
        }

        public async Task EnsureWorkflowIndexExistsAsync()
        {
         
            try
            {
                var exists = await _elasticClient.Indices.ExistsAsync(indexName);
                if (!exists.Exists)
                {
                    var response = await _elasticClient.Indices.CreateAsync(indexName, c => c
                        .Map<WorkflowLog>(m => m
                            .AutoMap()
                            .Properties(p => p
                                .Keyword(k => k.Name(n => n.WorkflowId))
                                .Keyword(k => k.Name(n => n.StepId))
                                .Keyword(k => k.Name(n => n.RequestId))
                                .Keyword(k => k.Name(n => n.UserCreatedId))
                                .Text(t => t.Name(n => n.UserName))
                                .Text(t => t.Name(n => n.Description))
                                .Text(t => t.Name(n => n.NewState))
                                .Text(t => t.Name(n => n.PreviousState))
                                .Keyword(k => k.Name(n => n.ActionType))
                                .Date(d => d.Name(n => n.CreatedAt))
                            )
                        )
                    );
                    var el = _elasticClient.Ping();
                    if (!el.IsValid)
                    {
                        Console.WriteLine("elastic connection error",el.DebugInformation);
                    }
                    else
                    {
                        _logger.LogInformation("Workflow index created successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create workflow index: {Info}", ex.Message);
                Console.WriteLine("Failed to create workflow index: {Info}", ex.Message);
                throw;
            }


        }
    }
}
