using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowSteps.Interfaces;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;


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

        public Task<ServiceResult<WorkflowStep>> GetAllWorkflowSteps()
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

        public async Task<ServiceResult<WorkflowStep>> UpsertWorkflowStep(int? id, WorkflowStepsCreatDto dto)
        {
            if ((dto?.WorkflowDefinitionId == null) || (dto.StepName == string.Empty))
            {
                return ServiceResult<WorkflowStep>
                  .Failure("Invalid data", 400, null, new[] { "The received argument is empty." });
            }
              
            WorkflowStep entity;

            if (id != null) // Update
            {
                entity = await _repository.GetByIdAsync((int)id);

                if (entity == null) ServiceResult<WorkflowStep>
                    .Failure("entity not found", 404, null, new[] { "Update failed." });

                _repository.UpdateEntity(entity);
                await _repository.SaveChangesAsync();

                return ServiceResult<WorkflowStep>.Success(entity, 200, "Updated successfully");
            }
            else // Create
            {
                entity =new WorkflowStep
                {
                    WorkflowDefinitionId = dto.WorkflowDefinitionId,
                    Order = dto.Order,
                    StepName = dto.StepName,
                    Role = dto.Role,
                    Editable = dto.Editable,
                    CreatedAt = DateTime.Now,
                    UserCreatedId = 1 // نمونه
                };
                await _repository.InsertAsync(entity);
                await _repository.SaveChangesAsync();

                return ServiceResult<WorkflowStep>.Success(entity, 201, "Created successfully");
            }
        }
    }
}
