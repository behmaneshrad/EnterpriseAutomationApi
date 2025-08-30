using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Logger.ElasticServices
{
    public interface IElasticWorkflowIndexService
    {
        Task EnsureWorkflowIndexExistsAsync();
    }
}
