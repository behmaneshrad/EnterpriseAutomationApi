using EnterpriseAutomation.Api.Controllers.BaseController;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.Requests;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Requests.Services;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApprovalController : BaseController<Request>
    {
        private readonly IApprovalService _approvalService;

        public ApprovalController(IRepository<Request> repository,
                                  IApprovalService approvalService)
        : base(repository)
        {
            _approvalService = approvalService;
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestDto dto)
        {
            try
            {
                var result = await _approvalService.ApproveAsync(id, dto);

                if (result.Error)
                {
                    return BadRequest(new
                    {
                        Message = result.Message,
                        Errors = result.Errors
                    });
                }

                return Ok(new
                {
                    Message = dto.IsApproved
                        ? "مرحله با موفقیت تایید شد."
                        : "مرحله با موفقیت رد شد.",
                    Request = result.Entity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "خطای داخلی سرور.", Details = ex.Message });
            }
        }
    }
}
