using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRepository<Request> _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestService(IRepository<Request> repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateRequestAsync(CreateRequestDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("توکن معتبر نیست یا کاربر لاگین نشده است.");

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "employee")
                throw new UnauthorizedAccessException("شما اجازه ثبت درخواست ندارید.");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن یافت نشد.");

            //  تبدیل به int
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("شناسه کاربر در توکن معتبر نیست یا عددی نیست.");

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
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
