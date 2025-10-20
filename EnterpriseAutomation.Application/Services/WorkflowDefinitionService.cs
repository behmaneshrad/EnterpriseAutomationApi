using Aqua.EnumerableExtensions;
using EnterpriseAutomation.Application.Extentions;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.WorkflowDefinitions;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace EnterpriseAutomation.Application.Services
{
    public class WorkflowDefinitionService : IWorkflowDefinitionsService
    {
        private readonly IRepository<WorkflowDefinition> _repository;
        private readonly ILogger<WorkflowDefinition> _logger;
        private readonly IUserService userService;
        private AuthorizationHandlerContext auth;

        public WorkflowDefinitionService(IRepository<WorkflowDefinition> repository, ILogger<WorkflowDefinition> logger, IUserService userService)
        {
            _repository = repository;
            _logger = logger;
            this.userService = userService;
           
        }
        public async Task<ServiceResult<WorkflowDefinitionCreateDto>> AddWorkflowDefinition(WorkflowDefinitionCreateDto wfDto)
        {
            if (wfDto == null)
            {
                return ServiceResult<WorkflowDefinitionCreateDto>.Failure("list is empty", 400);
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

            return ServiceResult<WorkflowDefinitionCreateDto>.Success(wfDto, 201, "workflow defintions add to database");
        }

        public Task DeleteWorkflowDefinition(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkflowDefinitionGetDto>> GetAllWorkflowDefinitionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>> GetAllWorkflowDefinitionsWithStepsAsync(int pageIndex, int pageSize, string? serchString)
        {

            var result = await _repository.GetAllAsync(
                p => p.Include(c => c.WorkflowSteps),
                asNoTracking: false);

            if (result == null)
            {
                _logger.LogWarning("Action: GetAllWorkflowDefinitionsWithStepsAsync Failure list empty");
                return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.Failure("list is empty", 400);
            }
            if (!serchString.IsNullOrEmpty())
            {
                result = result.Where(c => c.Name.Contains(serchString)).ToList();
            }

            var wtf = result.Select(c => new WorkflowDefinitionAndWorkflowStepDto
            {
                WorkflowDefinitionId = c.WorkflowDefinitionId,
                CreatedAt = c.CreatedAt,
                Name = c.Name,
                Description = c.Description,
                CreatedById = c.CreatedById,
                UpdatedAt = c.UpdatedAt,
                UpdatedById = c.UserCreatedId,
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
            var p = PaginatedList<WorkflowDefinitionAndWorkflowStepDto>
                .Create(wtf.AsQueryable(), pageIndex, pageSize);
            var log = new WorkflowLog()
            {
                ActionType = "Get all workflow definitions and relation with workflowstep",
                RequestId = 1,
                StepId = 1,
                WorkflowId = 1,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Description = "",
                PreviousState = "",
                NewState = "",
                UserName = ""
            }.ToString();
            _logger.LogInformation($"GetAllWorkflowDefinitionsWithStepsAsync success {log}");
            return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.SuccessPaginated(p, 200, "Restore all workflows and related steps.");
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

            if (result == null)
            {
                _logger.LogError($"WorkflowDefinition not found");
                return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>
                    .Failure("Not found result", 404);
            }

            var log = new WorkflowLog()
            {
                WorkflowId = 0,
                RequestId = 0,
                StepId = 0,
                UserId = Guid.NewGuid(),
                UserName = "",
                Description = "WorkflowDefinition not found",
                CreatedAt = DateTime.UtcNow,
                ActionType = "GetById"
            }.ToString();
            _logger.LogInformation($"WorkflowDefinition fetched successfully {log}");
            return ServiceResult<WorkflowDefinitionAndWorkflowStepDto>.Success(result, 200);
        }

        public Task<WorkflowDetailDto> GetWorkFlowById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<WorkflowDefinitionCreateDto>> UpdateWorkflowDefinition(int id, WorkflowDefinitionCreateDto wfDto)
        {
            try
            {
                if (wfDto == null) throw new ArgumentException(nameof(wfDto));

                var workflowdef = await _repository.GetByIdAsync(id);
                if (workflowdef == null)
                {
                    return ServiceResult<WorkflowDefinitionCreateDto>.Failure("not found workflow", 400);
                }

                workflowdef.Name = wfDto.Name;
                workflowdef.Description = wfDto.Description;
                workflowdef.UserModifyId = 1;
                workflowdef.UpdatedAt = DateTime.Now;

                _repository.UpdateEntity(workflowdef);
                await _repository.SaveChangesAsync();

                return ServiceResult<WorkflowDefinitionCreateDto>.Success(wfDto, 204, $"Update workflow: {wfDto.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"exception message: {ex.Message}");
                return ServiceResult<WorkflowDefinitionCreateDto>.Failure("Transient Failure ...", 500);
            }
        }

        public async Task<ServiceResult<WorkflowDefinition>> UpsertWorkflowDefinition(int? id, WorkflowDefinitionCreateDto entityDTO)
        {
            try
            {               

                WorkflowDefinition? entity;

                if (id != null) // Update
                {
                    entity = await _repository.GetByIdAsync((int)id);
                    if (entity == null)
                    {
                        _logger.LogError("Workflow not found");
                        return ServiceResult<WorkflowDefinition>.Failure("Workflow not found", 404);
                    }

                    entity.Description = entityDTO.Description;
                    entity.Name = entityDTO.Name;
                    entity.UpdatedAt = DateTime.Now;
                    entity.UserCreatedId = 3; // نمونه

                    _repository.UpdateEntity(entity);
                    await _repository.SaveChangesAsync();

                    var log = new WorkflowLog()
                    {
                        ActionType = "Upsert Workflow",
                        UserId = Guid.NewGuid(),
                        RequestId = 1,
                        StepId = 1,
                        WorkflowId = 1,
                        UserName = "hossein",
                        Description = entityDTO.Description,
                        CreatedAt = entity.CreatedAt,
                        UpdatedAt = entity.UpdatedAt,
                        NewState = "",
                        PreviousState = ""
                    }.ToString();
                    _logger.LogInformation($"Update workflow logger: {log}");
                    return ServiceResult<WorkflowDefinition>.Success(entity, 204, "Updated successfully");
                }
                else // Create
                {
                    entity = new WorkflowDefinition
                    {
                        Description = entityDTO.Description,
                        Name = entityDTO.Name,
                        CreatedAt = DateTime.Now,
                        CreatedById = entityDTO.userId,
                        UpdatedAt = DateTime.MinValue,
                        UserCreatedId = entityDTO.userId,
                    };

                    await _repository.InsertAsync(entity);
                    await _repository.SaveChangesAsync();

                    var log = new WorkflowLog()
                    {
                        ActionType = "Upsert Workflow",
                        UserId = Guid.NewGuid(),
                        RequestId = 1,
                        StepId = 1,
                        WorkflowId = 1,
                        UserName = "hossein",
                        Description = entityDTO.Description,
                        CreatedAt = entity.CreatedAt,
                        UpdatedAt = entity.UpdatedAt,
                        NewState = "",
                        PreviousState = ""
                    }.ToString();
                    _logger.LogInformation($"Created workflow logger: {log}");
                    return ServiceResult<WorkflowDefinition>.Success(entity, 201, "Created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception:{ex.Message}");
                return ServiceResult<WorkflowDefinition>.Failure(ex.Message, 500);
            }
        }
    }
}
