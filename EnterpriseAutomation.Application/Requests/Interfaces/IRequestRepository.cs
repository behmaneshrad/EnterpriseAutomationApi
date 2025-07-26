using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Application.Requests.Interfaces
{
    public interface IRequestRepository
    {
        Task<int> CreateAsync(Request request);
        Task<Request?> GetByIdAsync(int id);
        Task<List<Request>> GetAllAsync();
        Task SubmitAsync(int requestId, int employeeId);
    }
}