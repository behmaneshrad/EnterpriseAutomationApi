using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Api.Extensions;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.WorkflowSteps;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace EnterpriseAutomation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowStepsController : BaseController<WorkflowStep>
    {
        private readonly IWorkflowStepsService _stepsService; 
        private readonly IUserService userService;
        public WorkflowStepsController(IRepository<WorkflowStep> repository,
            IWorkflowStepsService stepsService,
            IUserService userService) : base(repository)
        {
            _stepsService = stepsService;
            this.userService = userService;
        }
        [HttpGet("GetAllWorkflowSteps/{pageIndex}/{pageSize}")]
        public async Task<ActionResult<ServiceResult<WorkflowStep>>> GetAllWorkflowSteps(int pageIndex=1,int pageSize=10)
        {
            var result = await _stepsService.GetAllWorkflowSteps(pageIndex,pageSize);
            return StatusCode(result.Status, result);
        }

        [HttpPost("UpsertWorkflowStep")]
        public async Task<ActionResult<ServiceResult<WorkflowStepsCreatDto>>> UpsertWorkflowStep(int? id, WorkflowStepsCreatDto workflowstepDto)
        {
            workflowstepDto.UserId = ClaimPrincipalExtensions.GetUserIdAsync(User, userService).Result;

            var result = await _stepsService.UpsertWorkflowStep(id, workflowstepDto);
            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }
            return StatusCode(result.Status, result);
        }
    }
}
