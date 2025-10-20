using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.Requests;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly IRepository<Request> _requestRepository;
        private readonly IRepository<WorkflowStep> _workflowStepRepository;
        private readonly IRepository<ApprovalStep> _approvalStepRepository;
        private readonly IRepository<WorkflowLog> _workflowLogRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApprovalService(
            IRepository<Request> requestRepository,
            IRepository<WorkflowStep> workflowStepRepository,
            IRepository<ApprovalStep> approvalStepRepository,
            IRepository<WorkflowLog> workflowLogRepository,
            IRepository<User> userRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _requestRepository = requestRepository;
            _workflowStepRepository = workflowStepRepository;
            _approvalStepRepository = approvalStepRepository;
            _workflowLogRepository = workflowLogRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResult<Request>> ApproveAsync(int requestId, ApproveRequestDto dto)
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal == null || !principal.Identity!.IsAuthenticated)
                return ServiceResult<Request>.Failure("کاربر احراز هویت نشده است.", 401);

            var approverName = principal.Identity?.Name ?? "Unknown";
            var nameId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;   // ممکن است GUID یا کلیدکلاک/رشته باشد

            //  یافتن کاربر تأییدکننده با چند حالت
            User? approver = null;
            if (!string.IsNullOrEmpty(nameId))
            {
                // حالت رایج: NameIdentifier به‌صورت GUID (Keycloak/ExternalGuid)
                approver = await _userRepository.GetFirstOrDefaultAsync(u => u.KeycloakId == nameId);
            }
            if (approver == null && !string.IsNullOrWhiteSpace(nameId))
            {
                // حالت دوم: NameIdentifier همان KeycloakId
                approver = await _userRepository.GetFirstOrDefaultAsync(u => u.KeycloakId == nameId);
            }
            if (approver == null && !string.IsNullOrWhiteSpace(approverName))
            {
                // حالت سوم: fallback با Username
                approver = await _userRepository.GetFirstOrDefaultAsync(u => u.Username == approverName);
            }
            if (approver == null)
                return ServiceResult<Request>.Failure("کاربر در سیستم یافت نشد.", 404);

            var request = await _requestRepository
                .GetQueryable(include: q => q.Include(r => r.ApprovalSteps))
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return ServiceResult<Request>.Failure("درخواست مورد نظر یافت نشد.", 404);

            if (request.ApprovalSteps == null || request.ApprovalSteps.Count == 0)
                return ServiceResult<Request>.Failure("برای این درخواست مرحله‌ای تعریف نشده است.", 400);

            var currentStep = request.ApprovalSteps.FirstOrDefault(s => s.StepId == request.CurrentStep);
            if (currentStep == null)
                return ServiceResult<Request>.Failure("مرحله جاری یافت نشد.", 400);

            if (currentStep.Status != ApprovalStatus.Pending)
                return ServiceResult<Request>.Failure("این مرحله قبلاً بررسی شده است.", 400);

            var stepMeta = await _workflowStepRepository.GetFirstOrDefaultAsync(ws =>
                ws.WorkflowDefinitionId == request.WorkflowDefinitionId &&
                ws.Order == request.CurrentStep);

            if (stepMeta == null)
                return ServiceResult<Request>.Failure("اطلاعات مرحله گردش کار یافت نشد.", 400);

            if (!string.IsNullOrWhiteSpace(stepMeta.Role))
            {
                var roles = stepMeta.Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var authorized = roles.Any(r => principal.IsInRole(r));
                if (!authorized)
                    return ServiceResult<Request>.Failure(
                        $"دسترسی رد شد. فقط کاربران با نقش‌های [{string.Join(", ", roles)}] مجاز به تأیید این مرحله هستند.", 403);
            }

            //  به‌روزرسانی مرحله و درخواست
            var prevState = currentStep.Status.ToString();

            currentStep.Status = dto.IsApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            currentStep.ApproverUserId = approver.UserId;   // int ← int
            currentStep.ApprovedAt = DateTime.UtcNow;

            if (dto.IsApproved)
            {
                var maxStepNumber = request.ApprovalSteps.Max(s => s.StepId);
                if (request.CurrentStep < maxStepNumber)
                {
                    request.CurrentStep++;
                    request.CurrentStatus = RequestStatus.InProgress;
                }
                else
                {
                    request.CurrentStatus = RequestStatus.Completed;
                }
            }
            else
            {
                request.CurrentStatus = RequestStatus.Rejected;
            }

            request.UpdatedAt = DateTime.UtcNow;


            try
            {
                var log = new WorkflowLog
                {
                    WorkflowId = request.WorkflowDefinitionId,
                    StepId = currentStep.StepId,          // شماره مرحله  ApprovalStepId
                    UserId = Guid.NewGuid(),   
                    UserName = approverName,
                    ActionType = dto.IsApproved ? "Approve" : "Reject",
                    Description = dto.Comment,
                    RequestId = request.RequestId,
                    PreviousState = prevState,
                    NewState = currentStep.Status.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _workflowLogRepository.InsertAsync(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkflowLog Error] {ex.Message}");
            }

            await _requestRepository.SaveChangesAsync();
            await _workflowLogRepository.SaveChangesAsync();

            return ServiceResult<Request>.Success(request, 200, "مرحله با موفقیت بررسی شد.");
        }
    }
}
