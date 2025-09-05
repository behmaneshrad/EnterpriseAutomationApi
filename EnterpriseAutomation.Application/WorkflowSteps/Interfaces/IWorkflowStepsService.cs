using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.WorkflowSteps.Interfaces
{
    public interface IWorkflowStepsService
    {
        public Task<ServiceResult<WorkflowStep>> GetAllWorkflowSteps(int pageIndex,int pageSize);

        public Task AddWorkflowStep(WorkflowStepsCreatDto workflowStepDto);

        public Task UpdateWorkflowStep(int id, WorkflowStepsCreatDto workflowStepDto);

        public Task<ServiceResult<WorkflowStep>> UpsertWorkflowStep(int? id, WorkflowStepsCreatDto dto);
    }
}