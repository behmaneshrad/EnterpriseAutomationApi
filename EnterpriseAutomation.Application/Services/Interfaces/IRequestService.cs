using EnterpriseAutomation.Application.Models.Requests;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Domain.Entities;


namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface IRequestService
    {
        Task CreateRequestAsync(CreateRequestDto dto);
        Task<IEnumerable<Request>> GetAllRequestsAsync();
        Task<Request?> GetRequestByIdAsync(int requestId);
        Task<bool> SubmitRequestAsync(SubmitRequestDto dto);
        Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(int workflowDefinitionId);
        Task<ServiceResult<Request>> GetFilteredRequestsAsync(string? status, string? role, Guid? createdBy, int pageIndex, int pageSize); //گرفتن درخواست ها با فیلتر
    }
}