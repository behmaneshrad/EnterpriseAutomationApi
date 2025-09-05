using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
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
        public WorkflowDefinitionsController(IRepository<WorkflowDefinition> repository,
            IWorkflowDefinitionsService definitionsService)
            : base(repository)
        {
            _definitionsService = definitionsService;
        }

        [HttpPost("UpsertWorkflow")]
        public async Task<ActionResult<ServiceResult<WorkflowDefinitionCreateDto>>> UpsertWorkflow(int? id, WorkflowDefinitionCreateDto entityDTO)
        {
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
    }
}
