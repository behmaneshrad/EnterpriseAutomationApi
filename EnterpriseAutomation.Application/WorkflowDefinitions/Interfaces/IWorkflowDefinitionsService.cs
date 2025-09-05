using EnterpriseAutomation.Application.ServiceResult;
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

        public Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetAllWorkflowDefinitionsWithStepsAsync(int pageIndex,int pageSize,string searchString);

        public Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetById(int id);

        public Task<WorkflowDetailDto> GetWorkFlowById(int id);

        public Task<ServiceResult<WorkflowDefinitionCreateDto>> AddWorkflowDefinition(WorkflowDefinitionCreateDto wfDto);

        public Task<ServiceResult<WorkflowDefinitionCreateDto>> UpdateWorkflowDefinition(int id, WorkflowDefinitionCreateDto wfDto);

        public Task<ServiceResult<WorkflowDefinition>> UpsertWorkflowDefinition(int? id, WorkflowDefinitionCreateDto entityDTO);

        public Task DeleteWorkflowDefinition(int id);
    }
}
