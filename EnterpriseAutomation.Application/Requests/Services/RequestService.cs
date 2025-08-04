using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRepository<Request> _repository;
        private readonly IRepository<WorkflowStep> _workflowStepRepository;
        private readonly IRepository<ApprovalStep> _approvalStepRepository; // اضافه شد
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestService(
            IRepository<Request> repository,
            IRepository<WorkflowStep> workflowStepRepository,
            IRepository<ApprovalStep> approvalStepRepository, // اضافه شد
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _workflowStepRepository = workflowStepRepository;
            _approvalStepRepository = approvalStepRepository; // اضافه شد
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Request>> GetFilteredRequestsAsync(RequestStatus? status, string? role, int? createdBy)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("توکن معتبر نیست یا کاربر لاگین نشده است.");

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(currentUserIdClaim, out int currentUserId);

            //  گرفتن Query با Include کامل
            var query = _repository.GetQueryable(
                include: q => q.Include(r => r.CreatedByUser)
                               .Include(r => r.ApprovalSteps),
                asNoTracking: true
            );

            //  محدودیت دسترسی برای Employee
            if (userRole == "employee")
            {
                query = query.Where(r => r.CreatedByUserId == currentUserId);
            }

            //  اعمال فیلترهای Optional
            if (status.HasValue)
                query = query.Where(r => r.CurrentStatus == status.Value);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(r => r.CreatedByUser != null && r.CreatedByUser.Role == role);

            if (createdBy.HasValue)
                query = query.Where(r => r.CreatedByUserId == createdBy.Value);

            return await query.ToListAsync();
        }


        public async Task CreateRequestAsync(CreateRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("توکن معتبر نیست یا کاربر لاگین نشده است.");

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "employee")
                throw new UnauthorizedAccessException("شما اجازه ثبت درخواست ندارید.");
                Console.WriteLine("test");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن یافت نشد.");

            //  تبدیل به int
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن معتبر نیست یا عددی نیست.");

            // ایجاد درخواست
            var request = new Request
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = userId,
                CurrentStatus = RequestStatus.Pending,
                CurrentStep = "Pending",
                WorkflowDefinitionId = 1,
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
                            StepId = step.Order, // استفاده از Order به عنوان شناسه مرحله
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
    }
}