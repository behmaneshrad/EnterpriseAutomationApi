using EnterpriseAutomation.Application.IRepository;
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
        public async Task AddWorkflowDefinition(WorkflowDefinitionCreateDto wfDto)
        {
            var workflowDefinition = new WorkflowDefinition()
            {
                Description = wfDto.Description,
                Name = wfDto.Name,
                CreatedAt = DateTime.Now,
                CreatedById = 3,
                UpdatedAt = DateTime.MinValue,
                UserCreatedId = 0,
            };

            await _repository.InsertAsync(workflowDefinition);
            await _repository.SaveChangesAsync();
        }

        public Task DeleteWorkflowDefinition(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowDefinitionGetDto>> GetAllWorkflowDefinitionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<WorkflowDefinitionAndWorkflowStepDto> GetById(int id)
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

            return result;
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
            workflowdef.UserModifyId = 3;
            workflowdef.UpdatedAt = DateTime.Now;

            _repository.UpdateEntity(workflowdef);
            await _repository.SaveChangesAsync();

        }
    }
}
