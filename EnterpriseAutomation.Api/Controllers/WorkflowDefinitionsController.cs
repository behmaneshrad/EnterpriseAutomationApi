using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers
{
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

        [HttpGet("GetWorkflowDefinitionsAndStepById/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _definitionsService.GetById(id);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("AddAndupdateWorkflow")]
        public async Task<IActionResult> AddAndupdateWorkflow(int? id, WorkflowDefinitionCreateDto entityDTO)
        {
            if (id != null)
            {
                await _definitionsService.UpdateWorkflowDefinition((int)id, entityDTO);
                return StatusCode(202, new { message = "update" });
            }
               

            //Create Workflow
            if (entityDTO != null)
            {
                await _definitionsService.AddWorkflowDefinition(entityDTO);
                return StatusCode(201, new { message = "insert is success" });
            }
               

            return BadRequest(new { message = "invalid data" });
        }
    }
}
