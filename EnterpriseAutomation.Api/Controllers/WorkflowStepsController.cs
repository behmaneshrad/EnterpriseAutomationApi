using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowSteps.Interfaces;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace EnterpriseAutomation.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowStepsController : BaseController<WorkflowStep>
    {
        private readonly IWorkflowStepsService _stepsService; 
        public WorkflowStepsController(IRepository<WorkflowStep> repository,
            IWorkflowStepsService stepsService) : base(repository)
        {
            _stepsService = stepsService;
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
            var result = await _stepsService.UpsertWorkflowStep(id, workflowstepDto);
            if (result.Error)
            {
                return StatusCode(result.Status, result);
            }
            return StatusCode(result.Status, result);
        }
    }
}
