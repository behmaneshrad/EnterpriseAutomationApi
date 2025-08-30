using EnterpriseAutomation.Domain.Entities;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Logger.WorkflowLogger
{
    public class WorkflowLogService : IWorkflowLogService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<WorkflowLogService> _logger;
        public WorkflowLogService(IElasticClient elasticClient, ILogger<WorkflowLogService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }
        public async Task LogAsync(WorkflowLog log)
        {
            var response = await _elasticClient
                .IndexAsync(log, c => c.Index("workflow-logs"));
            if (!response.IsValid) 
            {
                _logger.LogError("Failed to index workflow log: {Info}", response.DebugInformation);
            }
        }
    }
}
