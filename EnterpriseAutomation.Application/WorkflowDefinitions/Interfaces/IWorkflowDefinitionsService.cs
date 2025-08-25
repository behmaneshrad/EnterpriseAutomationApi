using EnterpriseAutomation.Application.ServiceResults;
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
        public Task<IEnumerable<WorkflowDefinitionGetDto>> GetAllWorkflowDefinitionsAsync();

        public Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetById(int id);

        public Task<WorkflowDetailDto> GetWorkFlowById(int id);

        public Task<ServiceResult<WorkflowDefinition>> AddWorkflowDefinition(WorkflowDefinitionCreateDto wfDto);

        public Task UpdateWorkflowDefinition(int id, WorkflowDefinitionCreateDto wfDto);

        public Task<ServiceResult<WorkflowDefinition>> UpsertWorkflowDefinition(int? id, WorkflowDefinitionCreateDto entityDTO);

        public Task DeleteWorkflowDefinition(int id);
    }
}
