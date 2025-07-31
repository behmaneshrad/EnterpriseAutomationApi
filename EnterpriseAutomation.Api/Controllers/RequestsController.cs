using EnterpriseAutomation.Api.Controllers;
using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Application.Requests.Services;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseAutomation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : BaseController<Request>
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService,IRepository<Request> repository)
        :base(repository)
    {
        _requestService = requestService;
    }

    //// 3. ایجاد یک Request جدید (POST)
    //[HttpPost]
    //public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    try
    //    {
    //        await _requestService.CreateRequestAsync(dto);
    //        return StatusCode(201, new
    //        {
    //            Message="باموفقیت درخواست شما ثبت شد"
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { Message = "خطا در ایجاد درخواست", Error = ex.Message });
    //    }
    //}

    //[HttpGet]
    //public async Task<IActionResult> GetAllRequests()
    //{
    //    try
    //    {
    //        var requests = await _requestService.GetAllRequestsAsync();

    //        // تبدیل به DTO برای جلوگیری از loop و سبک‌سازی خروجی
    //        var result = requests.Select(r => new
    //        {
    //            r.RequestId,
    //            r.Title,
    //            r.Description,
    //            r.CreatedByUserId,
    //            r.CurrentStatus,
    //            r.CurrentStep,
    //            r.WorkflowDefinitionId,
    //            User = new
    //            {
    //                r.CreatedByUser.UserId,
    //                r.CreatedByUser.Username,
    //                r.CreatedByUser.Role
    //            },
    //            ApprovalSteps = r.ApprovalSteps.Select(s => new
    //            {
    //                s.ApprovalStepId,
    //                s.Status
    //            }).ToList()
    //        });

    //        return Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { Message = "خطا در دریافت درخواست‌ها", Error = ex.Message });
    //    }
    //}


    //// 4. ارسال درخواست توسط کارمند (POST)
    //[HttpPost("submit")]
    //public async Task<IActionResult> SubmitRequest([FromBody] SubmitRequestDto dto)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    try
    //    {
    //        var result = await _requestService.SubmitRequestAsync(dto);
    //        if (result)
    //            return Ok(new { Message = "درخواست با موفقیت ارسال شد" });
    //        else
    //            return BadRequest(new { Message = "خطا در ارسال درخواست" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { Message = "خطا در ارسال درخواست", Error = ex.Message });
    //    }
    //}

    //// 5. دریافت اطلاعات یک درخواست خاص با جزئیات بیشتر (GET by ID)
    //[HttpGet("{requestId:int}")]
    //public async Task<IActionResult> GetRequestById(int requestId)
    //{
    //    try
    //    {
    //        var request = await _requestService.GetRequestByIdAsync(requestId);
    //        if (request == null)
    //            return NotFound(new { Message = "درخواست یافت نشد" });

    //        var dto = new RequestDto
    //        {
    //            RequestId = request.RequestId,
    //            Title = request.Title,
    //            Description = request.Description,
    //            CurrentStatus = request.CurrentStatus,
    //            CurrentStep = request.CurrentStep,
    //            UserId = request.UserCreatedId,
    //            Username = request.CreatedByUser.Username
    //        };

    //        return Ok(dto);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { Message = "خطا در دریافت درخواست", Error = ex.Message });
    //    }
    //}
}