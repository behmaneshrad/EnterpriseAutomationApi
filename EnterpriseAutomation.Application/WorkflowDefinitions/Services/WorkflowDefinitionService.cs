using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
using EnterpriseAutomation.Domain.Entities;
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
        public async Task AddWorkflowDefinition(WorkflowDefinitionDto wfDto)
        {
            var workflowDefinition = new WorkflowDefinition()
            {
                CreatedAt = DateTime.Now,
                Description = wfDto.Description,
                Name = wfDto.Name,
                CreatedById= wfDto.CreatedById,
                UpdatedAt = DateTime.Now,
                UserCreatedId=wfDto.UpdatedById,
            };

           await _repository.InsertAsync(workflowDefinition);
           await _repository.SaveChangesAsync();
        }

        public Task DeleteWorkflowDefinition(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowDefinitionDto>> GetAllWorkflowDefinitionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowDetailDto> GetWorkFlowById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
