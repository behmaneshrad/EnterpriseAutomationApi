using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;

namespace EnterpriseAutomation.Application.Requests.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRepository<Request> _repository;

        public RequestService(IRepository<Request> repository)
        {
            _repository = repository;
        }

        public async Task CreateRequestAsync(CreateRequestDto dto)
        {
            var request = new Request
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = dto.CreatedBy,
                CurrentStatus = RequestStatus.Draft,
                CurrentStep = "Initial Creation",
                WorkflowDefinitionId = 1, // فعلاً یک مقدار پیش‌فرض
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
                //await _requestRepository.SubmitAsync(dto.RequestId, dto.EmployeeId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}