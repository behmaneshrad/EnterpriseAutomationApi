using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;

        public RequestService(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<int> CreateRequestAsync(CreateRequestDto dto)
        {
            var request = new Request
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = dto.CreatedBy,
                CurrentStatus = RequestStatus.Draft,
                CurrentStep = "Initial Creation",
                WorkflowId = 1, // فعلاً یک مقدار پیش‌فرض
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _requestRepository.CreateAsync(request);
        }

        public async Task<List<Request>> GetAllRequestsAsync()
        {
            return await _requestRepository.GetAllAsync();
        }

        public async Task<Request?> GetRequestByIdAsync(int requestId)
        {
            return await _requestRepository.GetByIdAsync(requestId);
        }

        public async Task<bool> SubmitRequestAsync(SubmitRequestDto dto)
        {
            try
            {
                await _requestRepository.SubmitAsync(dto.RequestId, dto.EmployeeId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}