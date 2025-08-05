using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowDefinitionsController : BaseController<WorkflowDefinition>
    {
        private readonly IWorkflowDefinitionsService _definitionsService;
        public WorkflowDefinitionsController(IRepository<WorkflowDefinition> repository, IWorkflowDefinitionsService definitionsService) : base(repository)
        {
            _definitionsService = definitionsService;
        }
    }
}
