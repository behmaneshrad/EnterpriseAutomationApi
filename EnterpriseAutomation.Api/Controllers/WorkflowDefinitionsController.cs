using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Api.Extensions;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.WorkflowDefinitions;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowDefinitionsController :
        BaseController<WorkflowDefinition>
    {
        private readonly IWorkflowDefinitionsService _definitionsService;
        private readonly IUserService userService;

        public WorkflowDefinitionsController(IRepository<WorkflowDefinition> repository,
            IWorkflowDefinitionsService definitionsService, IUserService userService)
            : base(repository)
        {
            _definitionsService = definitionsService;
            this.userService = userService;
        }

        [HttpPost("UpsertWorkflow")]
        public async Task<ActionResult<ServiceResult<WorkflowDefinitionCreateDto>>> UpsertWorkflow(int? id, WorkflowDefinitionCreateDto entityDTO)
        {
            entityDTO.userId = ClaimPrincipalExtensions.GetUserIdAsync(User,userService).Result;
            var result = await _definitionsService.UpsertWorkflowDefinition(id, entityDTO);
            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }
            return StatusCode(result.Status, result);
        }

        [HttpGet("GetWorkflowDefinitionsAndStepById/{id}")]
        public async Task<ActionResult<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>>> Get(int id)
        {
            var result = await _definitionsService.GetById(id);

            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }

            return StatusCode(result.Status, result);
        }


        [HttpGet("GetAllWorkflowDefinitionsWithSteps")]
        public async Task<ActionResult<ServiceResult<WorkflowDefinitionAndWorkflowStepDto>>> GetAllWorkflowDefinitionsWithSteps(int pageIndex = 1, int pageSize = 10,string searchString="")
        {
            var result = await _definitionsService.GetAllWorkflowDefinitionsWithStepsAsync(pageIndex, pageSize,searchString);
            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }
            return StatusCode(result.Status, result);
        }


        [HttpGet("GetWorkflowRequest/{id}")]
        public async Task<ActionResult<ServiceResult<WorkflowRequestDto>>> GetWorkflowAndRequestById(int id,int pageIndex=1,int pageSize=10)
        {
            var result=await _definitionsService.GetWorkflowAndRequestById(id,pageIndex,pageSize);
            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }
            return StatusCode(result.Status, result);
        }
    }
}
