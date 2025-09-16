using EnterpriseAutomation.Application.Models.WorkflowSteps;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IWorkflowStepsService
    {
        public Task<ServiceResult<WorkflowStep>> GetAllWorkflowSteps(int pageIndex,int pageSize);

        public Task AddWorkflowStep(WorkflowStepsCreatDto workflowStepDto);

        public Task UpdateWorkflowStep(int id, WorkflowStepsCreatDto workflowStepDto);

        public Task<ServiceResult<WorkflowStep>> UpsertWorkflowStep(int? id, WorkflowStepsCreatDto dto);
    }
}