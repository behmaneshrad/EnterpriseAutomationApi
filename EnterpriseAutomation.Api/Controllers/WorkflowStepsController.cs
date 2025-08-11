using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowSteps.Interfaces;
using EnterpriseAutomation.Application.WorkflowSteps.Models;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;


namespace EnterpriseAutomation.Api.Controllers
{
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

        [HttpPost("UpsertWorkflowStep")]
        public async Task<IActionResult> UpsertWorkflowStep(int id, WorkflowStepsCreatDto workflowstepDto)
        {
            if ((id == 0)|| (id == null))
            {
                await _stepsService.AddWorkflowStep(workflowstepDto);
                return Ok();
            }
            await _stepsService.UpdateWorkflowStep(id,workflowstepDto);
            return Ok();
        }
    }
}
