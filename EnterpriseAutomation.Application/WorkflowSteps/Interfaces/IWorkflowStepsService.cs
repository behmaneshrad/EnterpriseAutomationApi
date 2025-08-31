using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowSteps.Interfaces
{
    public interface IWorkflowStepsService
    {
        public Task<ServiceResult<WorkflowStep>> GetAllWorkflowSteps();

        public Task AddWorkflowStep(WorkflowStepsCreatDto workflowStepDto);

        public Task UpdateWorkflowStep(int id, WorkflowStepsCreatDto workflowStepDto);

        public Task<ServiceResult<WorkflowStep>> UpsertWorkflowStep(int? id, WorkflowStepsCreatDto dto);
    }
}