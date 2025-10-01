using EnterpriseAutomation.Api.Controllers;
using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using EnterpriseAutomation.Domain.Entities.Enums;
using EnterpriseAutomation.Api.Security;
using EnterpriseAutomation.Domain.Entities.Policy;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Application.Models.Requests;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : BaseController<Request>
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRepository<Request> repository,
             IRequestService _requestService) : base(repository)
        {

            this._requestService = _requestService;
        }         


       [HttpGet("RequestList/{status}/{createdBy}")]
        public async Task<ActionResult<ServiceResult<List<RequestDto>>>> RequestListFilter(
        string? currentStatus, Guid? createdBy)
        {
            var requests = await _requestService.GetFilteredRequestsAsync(currentStatus, null, createdBy, 1, 10);

            var dtoList = requests.Entities.Select(r => new RequestDto
            {
                RequestId = r.RequestId,
                Title = r.Title,
                Description = r.Description,
                CurrentStatus = r.CurrentStatus,
                CurrentStep = r.CurrentStep,
                UserId = r.CreatedByUserId
            }).ToList();

            return ServiceResult<List<RequestDto>>.Success(dtoList, 1, "success");
        }


        // 3. ایجاد یک Request جدید (POST)
        //[Authorize]
        //[RequiresPermission("requests", "create")]
        [HttpPost("create")] // حتماً متد HTTP مشخص باشد
        public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _requestService.CreateRequestAsync(dto);
                return StatusCode(201, new { Message = "باموفقیت درخواست شما ثبت شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در ایجاد درخواست", Error = ex.Message });
            }
        }

        [HttpGet]        
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var requests = await _requestService.GetAllRequestsAsync();

                // تبدیل به DTO برای جلوگیری از loop و سبک‌سازی خروجی
                var result = requests.Select(r => new
                {
                    r.RequestId,
                    r.Title,
                    r.Description,
                    CreatedByUserId = r.CreatedByUserId, 
                    r.CurrentStatus,
                    r.CurrentStep,
                    r.WorkflowDefinitionId,
                    //User = new
                    //{
                    //    r.CreatedByUser.UserId,
                    //    r.CreatedByUser.Username,
                    //    r.CreatedByUser.Role
                    //},
                    ApprovalSteps = r.ApprovalSteps.Select(s => new
                    {
                        s.ApprovalStepId,
                        s.Status
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در دریافت درخواست‌ها", Error = ex.Message });
            }
        }

        // 4. ارسال درخواست توسط کارمند (POST)
       
        [HttpPost("submit")] 
        public async Task<IActionResult> SubmitRequest([FromBody] SubmitRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _requestService.SubmitRequestAsync(dto);
                if (result)
                    return Ok(new { Message = "درخواست با موفقیت ارسال شد" });
                else
                    return BadRequest(new { Message = "خطا در ارسال درخواست" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در ارسال درخواست", Error = ex.Message });
            }
        }

        // 5. دریافت اطلاعات یک درخواست خاص با جزئیات بیشتر (GET by ID)
        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetRequestById(int requestId)
        {
            try
            {
                var request = await _requestService.GetRequestByIdAsync(requestId);
                if (request == null)
                    return NotFound(new { Message = "درخواست یافت نشد" });

                // TODO : UserName
                var dto = new RequestDto
                {
                    RequestId = request.RequestId,
                    Title = request.Title,
                    Description = request.Description,
                    CurrentStatus = request.CurrentStatus,
                    CurrentStep = request.CurrentStep,
                    UserId = request.CreatedByUserId,
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در دریافت درخواست", Error = ex.Message });
            }
        }

        // 6. دریافت مراحل گردش کار برای یک WorkflowDefinition خاص (GET)
        //[Authorize(Policy = "User")]
        // TODO : this Process should be run after creating request but for now it just works with workflow definitoion id
        [HttpGet("workflow/{workflowDefinitionId:int}")]
        public async Task<IActionResult> GetWorkflowSteps(int workflowDefinitionId)
        {
            try
            {
                var workflowSteps = await _requestService.GetWorkflowStepsAsync(workflowDefinitionId);

                if (workflowSteps == null || !workflowSteps.Any())
                    return NotFound(new { Message = "مراحل گردش کار یافت نشد" });

                var result = new
                {
                    WorkflowDefinitionId = workflowDefinitionId,
                    WorkflowSteps = workflowSteps
                        .Select((step, index) => $"{index + 1}- {step.StepName}")
                        .ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در دریافت مراحل گردش کار", Error = ex.Message });
            }
        }

        //  دریافت لیست درخواست‌ها با فیلترهای status, role, createdBy
        [HttpGet("list")]
        public async Task<IActionResult> GetFilteredRequests([FromQuery] string? status,
            [FromQuery] string? role, [FromQuery] Guid? createdBy)
        {
            try
            {
                var requests = await _requestService.GetFilteredRequestsAsync(status, role, createdBy, 1, 10);


                var result = requests.Entities.Select(r => new RequestDto
                {
                    RequestId = r.RequestId,
                    Title = r.Title,
                    Description = r.Description,
                    CurrentStatus = r.CurrentStatus,
                    CurrentStep = r.CurrentStep,
                    UserId = r.CreatedByUserId,
                    //Username = r.CreatedByUser?.Username ?? "Unknown"
                });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطا در دریافت لیست درخواست‌ها", Error = ex.Message });
            }
        }

  
        
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestDto dto)
        {
            try
            {
                await _requestService.ApproveAsync(id, dto.IsApproved, dto.Comment);

                return Ok(new
                {
                    Message = dto.IsApproved
                        ? "مرحله با موفقیت تایید شد."
                        : "مرحله با موفقیت رد شد."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطای داخلی سرور", Error = ex.Message });
            }
        }


    }
}