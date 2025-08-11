using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowSteps.Interfaces;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowSteps.Services
{
    public class WorkflowStepsService : IWorkflowStepsService
    {
        private readonly IRepository<WorkflowStep> _repository;
        public WorkflowStepsService(IRepository<WorkflowStep> repository)
        {
            _repository = repository;
        }
        public async Task AddWorkflowStep(WorkflowStepsCreatDto workflowStepDto)
        {
            var workflowStep = new WorkflowStep()
            {
                CreatedAt = DateTime.Now,
                UserCreatedId = 3,
                StepName = workflowStepDto.StepName,
                Role = workflowStepDto.Role,
                Order = workflowStepDto.Order,
                WorkflowDefinitionId = workflowStepDto.WorkflowDefinitionId,
            };

            await _repository.InsertAsync(workflowStep);
            await _repository.SaveChangesAsync();
        }

        public Task<IEnumerable<WorkflowStep>> GetAllWorkflowSteps()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateWorkflowStep(int id, WorkflowStepsCreatDto workflowStepDto)
        {
            var workflowStep = await _repository.GetByIdAsync(id);
            if (workflowStep == null) throw new ArgumentException(nameof(workflowStep));

            workflowStep.UpdatedAt = DateTime.Now;
            workflowStep.Editable = workflowStepDto.Editable;
            workflowStep.Order = workflowStepDto.Order;
            workflowStep.Role = workflowStepDto.Role;
            workflowStep.StepName = workflowStepDto.StepName;
            workflowStep.UserModifyId = 3;

            _repository.UpdateEntity(workflowStep);
            await _repository.SaveChangesAsync();
        }
    }
}
