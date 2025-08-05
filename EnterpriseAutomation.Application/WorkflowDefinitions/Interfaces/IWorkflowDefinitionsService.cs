using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces
{
    public interface IWorkflowDefinitionsService
    {
        public Task<IEnumerable<WorkflowDefinitionDto>> GetAllWorkflowDefinitionsAsync();

        public Task<WorkflowDetailDto> GetWorkFlowById(int id);

        public Task AddWorkflowDefinition(WorkflowDefinitionDto wfDto);
        public Task DeleteWorkflowDefinition(int id);
    }
}
