using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.WorkflowDefinitions.Services
{
    public class WorkflowDefinitionService : IWorkflowDefinitionsService
    {
        private readonly IRepository<WorkflowDefinition> _repository;

        public WorkflowDefinitionService(IRepository<WorkflowDefinition> repository)
        {
            _repository = repository;
        }
        public async Task<ServiceResult<WorkflowDefinition>> AddWorkflowDefinition(WorkflowDefinitionCreateDto wfDto)
        {
            if (wfDto == null)
            {
                return ServiceResult<WorkflowDefinition>.Failure("list is empty", 400);


            }
            var workflowDefinition = new WorkflowDefinition()
            {
                Description = wfDto.Description,
                Name = wfDto.Name,
                CreatedAt = DateTime.Now,
                CreatedById = 1,
                UpdatedAt = DateTime.MinValue,
                UserCreatedId = 0,
            };

            await _repository.InsertAsync(workflowDefinition);
            await _repository.SaveChangesAsync();

            return ServiceResult<WorkflowDefinition>.Success(workflowDefinition, 201, "workflow defintions add to database");
        }

        public Task DeleteWorkflowDefinition(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowDefinitionGetDto>> GetAllWorkflowDefinitionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetAllWorkflowDefinitionsWithStepsAsync()
        {
            var result = await _repository.GetAllAsync(
                p => p.Include(c=>c.WorkflowSteps),
                asNoTracking: false);

            if (result == null)
                return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.Failure("list is empty",400);
            
            var wtf = result.Select(c => new WorkflowDefinitionAndWorkflowStepDto
            {
                WorkflowDefinitionId = c.WorkflowDefinitionId,
                CreatedAt = c.CreatedAt,
                Name = c.Name,
                Description = c.Description,
                CreatedById = c.CreatedById,
                UpdatedAt=c.UpdatedAt,
                UpdatedById=c.UserCreatedId,
                WorkflowStepDto = c.WorkflowSteps.Select(w => new WorkflowStepDto
                {
                    WorkflowDefinitionId = w.WorkflowDefinitionId,
                    WorkflowStepId = w.WorkflowStepId,
                    Order = w.Order,
                    Role = w.Role,
                    StepName = w.StepName,
                    Editable = w.Editable
                }).ToList()
            });

            return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.SuccessList(wtf, 200, "Restore all workflows and related steps.");
        }

        public async Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetById(int id)
        {
            var result = await _repository.GetQueryable(
                q => q.Include(x => x.WorkflowSteps)
                .Where(x => x.WorkflowDefinitionId == id)
            ).Select(entity => new WorkflowDefinitionAndWorkflowStepDto
            {
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                Name = entity.Name,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                CreatedById = entity.CreatedById,
                UpdatedAt = entity.UpdatedAt ?? DateTime.MinValue,
                UpdatedById = entity.UserModifyId ?? 0,
                WorkflowStepDto = entity.WorkflowSteps.Select(x => new WorkflowStepDto
                {
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowStepId = x.WorkflowStepId,
                    Order = x.Order,
                    Role = x.Role,
                    StepName = x.StepName
                }).ToList()
            }).FirstOrDefaultAsync();

            if (result == null) return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.Failure("Not found result", 404);

            return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.Success(result, 200);
        }

        public Task<WorkflowDetailDto> GetWorkFlowById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateWorkflowDefinition(int id, WorkflowDefinitionCreateDto wfDto)
        {
            if (wfDto == null) throw new ArgumentException(nameof(wfDto));

            var workflowdef = await _repository.GetByIdAsync(id);
            if (workflowdef == null) throw new ArgumentException(nameof(workflowdef));

            workflowdef.Name = wfDto.Name;
            workflowdef.Description = wfDto.Description;
            workflowdef.UserModifyId = 1;
            workflowdef.UpdatedAt = DateTime.Now;

            _repository.UpdateEntity(workflowdef);
            await _repository.SaveChangesAsync();

        }

        public async Task<ServiceResult<WorkflowDefinition>> UpsertWorkflowDefinition(int? id, WorkflowDefinitionCreateDto entityDTO)
        {
            if ((entityDTO.Description == string.Empty)||(entityDTO.Name==string.Empty))
                return ServiceResult<WorkflowDefinition>.Failure("Invalid data", 400);

            WorkflowDefinition entity;

            if (id != null) // Update
            {
                entity = await _repository.GetByIdAsync((int)id);
                if (entity == null)
                    return ServiceResult<WorkflowDefinition>.Failure("Workflow not found", 404);

                entity.Description = entityDTO.Description;
                entity.Name = entityDTO.Name;
                entity.UpdatedAt = DateTime.Now;
                entity.UserCreatedId = 1; // نمونه

                _repository.UpdateEntity(entity);
                await _repository.SaveChangesAsync();

                return ServiceResult<WorkflowDefinition>.Success(entity, 204, "Updated successfully");
            }
            else // Create
            {
                entity = new WorkflowDefinition
                {
                    Description = entityDTO.Description,
                    Name = entityDTO.Name,
                    CreatedAt = DateTime.Now,
                    CreatedById = 1,
                    UpdatedAt = DateTime.MinValue,
                    UserCreatedId = 0
                };

                await _repository.InsertAsync(entity);
                await _repository.SaveChangesAsync();

                return ServiceResult<WorkflowDefinition>.Success(entity, 201, "Created successfully");
            }
        }
    }
}
