using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Linq;
using EnterpriseAutomation.Domain.Enums;
using EnterpriseAutomation.Infrastructure.Utilities;
using EnterpriseAutomation.Application.ServiceResult;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRepository<Request> _repository;
        private readonly IRepository<WorkflowStep> _workflowStepRepository;
        private readonly IRepository<ApprovalStep> _approvalStepRepository; // اضافه شد
        private readonly IRepository<WorkflowLog> _workflowLogRepository; // اضافه شد
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestService(
            IRepository<Request> repository,
            IRepository<WorkflowStep> workflowStepRepository,
            IRepository<ApprovalStep> approvalStepRepository, // اضافه شد
            IRepository<WorkflowLog> workflowLogRepository, // اضافه شد
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _workflowStepRepository = workflowStepRepository;
            _approvalStepRepository = approvalStepRepository; // اضافه شد
            _workflowLogRepository = workflowLogRepository;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task CreateRequestAsync(CreateRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("توکن معتبر نیست یا کاربر لاگین نشده است.");

            // چندین روش برای پیدا کردن sub
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value
                   ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userGuid))
            {
                // برای debug - تمام claims را لاگ کنید
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
                }

                throw new UnauthorizedAccessException("شناسه کاربر (sub) در توکن معتبر نیست.");
            }

            var request = new Request
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = userGuid,                 // از توکن
                CurrentStatus = RequestStatus.Pending,
                CurrentStep = 1,
                WorkflowDefinitionId = 4,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _repository.InsertAsync(request);
            await _repository.SaveChangesAsync();

            // حالا ApprovalStep ها را ایجاد می‌کنیم
            try
            {
                // دریافت مراحل workflow
                var workflowSteps = await GetWorkflowStepsAsync(request.WorkflowDefinitionId);
                if (workflowSteps != null && workflowSteps.Any())
                {
                    var approvalSteps = new List<ApprovalStep>();
                    foreach (var step in workflowSteps)
                    {
                        var approvalStep = new ApprovalStep
                        {
                            StepId = step.Order,
                            RequestId = request.RequestId,
                            ApproverUserId = null,
                            Status = ApprovalStatus.Pending,
                            ApprovedAt = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        approvalSteps.Add(approvalStep);
                    }

                    // ذخیره تمام ApprovalSteps
                    await _approvalStepRepository.InsertAsync(approvalSteps);
                    await _approvalStepRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // اگر ایجاد ApprovalStep ها با خطا مواجه شد، درخواست را حذف کنیم
                await _repository.DeleteByIdAsync(request.RequestId);
                await _repository.SaveChangesAsync();
                throw new Exception($"خطا در ایجاد مراحل تایید: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Request>> GetAllRequestsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Request?> GetRequestByIdAsync(int requestId)
        {
            return await _repository.GetByIdAsync(requestId);
        }

        public async Task<bool> SubmitRequestAsync(SubmitRequestDto dto)
        {
            try
            {
                // TODO: Implement actual submission logic
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(int workflowDefinitionId)
        {
            try
            {
                var workflowSteps = await _workflowStepRepository
                    .GetWhereAsync(ws => ws.WorkflowDefinitionId == workflowDefinitionId);

                return workflowSteps
                    .OfType<WorkflowStep>()
                    .OrderBy(ws => ws.Order)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در دریافت مراحل گردش کار برای تعریف {workflowDefinitionId}", ex);
            }
        }



        public async Task ApproveAsync(int requestId, bool isApproved, string? comment)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("کاربر معتبر نیست.");

            // شناسه کاربر تاییدکننده (Guid از توکن)
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var approverGuid))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن معتبر نیست.");

            var approverName = user.Identity?.Name
                ?? user.FindFirst("preferred_username")?.Value
                ?? "Unknown";

            // لود درخواست + مراحل
            var request = await _repository
                .GetQueryable(include: q => q.Include(r => r.ApprovalSteps))
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                throw new KeyNotFoundException("درخواست پیدا نشد.");

            var currentStep = request.ApprovalSteps
                .OrderBy(s => s.StepId)
                .FirstOrDefault(s => s.StepId == request.CurrentStep);

            if (currentStep == null)
                throw new InvalidOperationException("مرحله جاری یافت نشد.");

            if (currentStep.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException("این مرحله قبلاً بررسی شده است.");

            //  چک نقش
            var stepMeta = await _workflowStepRepository.GetFirstOrDefaultAsync(ws =>
                ws.WorkflowDefinitionId == request.WorkflowDefinitionId &&
                ws.Order == request.CurrentStep);

            if (stepMeta == null)
                throw new InvalidOperationException("اطلاعات مرحله گردش کار یافت نشد.");

            // بررسی نقش کاربر با نقش موردنیاز مرحله
            if (!string.IsNullOrWhiteSpace(stepMeta.Role))
            {
                if (!user.IsInRole(stepMeta.Role))
                    throw new UnauthorizedAccessException($"شما نقش مورد نیاز این مرحله ({stepMeta.Role}) را ندارید.");
            }

            var prevState = currentStep.Status.ToString();

            currentStep.Status = isApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            currentStep.ApproverUserId = approverGuid;
            currentStep.ApprovedAt = DateTime.UtcNow;

            // تغییر وضعیت درخواست
            if (isApproved)
            {
                var maxStep = request.ApprovalSteps.Max(s => s.StepId);
                if (request.CurrentStep < maxStep)
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

            // ثبت لاگ بعد از تغییر 
            var newState = currentStep.Status.ToString();
            var log = new WorkflowLog
            {
                WorkflowId = request.WorkflowDefinitionId,
                StepId = currentStep.StepId,
                UserId = approverGuid,
                UserName = approverName,
                ActionType = isApproved ? ActionType.Approve : ActionType.Reject,
                Description = comment,
                RequestId = request.RequestId,
                PreviousState = prevState,
                NewState = newState,
                CreatedAt = DateTime.UtcNow
            };
            await _workflowLogRepository.InsertAsync(log);

            _approvalStepRepository.UpdateEntity(currentStep);
            _repository.UpdateEntity(request);
            await _workflowLogRepository.SaveChangesAsync();
        }

        public Task<IEnumerable<RequestDto>> RequestListFilter(string CurrentStatus, Guid createdBy)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<Request>> GetFilteredRequestsAsync(string? status, string? role, Guid? createdBy, int pageIndex, int pageSize)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                ServiceResult<Request>.Failure("Authenticated Failure ", 401);
            Guid.TryParse(user.FindFirst("sub")?.Value, out var currentUserGuid);

            var query = _repository.GetQueryable(
                include: q => q.Include(r => r.ApprovalSteps),
                asNoTracking: true
            );

            if (user.IsInRole("employee") && currentUserGuid != Guid.Empty)
            {
                query = query.Where(r => r.CreatedByUserId == currentUserGuid);
            }

            if (status != null)
                query = query.Where(r => r.CurrentStatus == status);


            if (createdBy.HasValue)
                query = query.Where(r => r.CreatedByUserId == createdBy.Value);

            var p = await PaginatedList<Request>.CreateAsync(query, pageIndex, pageSize);

            return ServiceResult<Request>.SuccessPaginated(p,200,"Request with Filter");
        }
    }
}