using EnterpriseAutomation.Application.Models.Requests;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Domain.Entities;

public interface IApprovalService
{
    Task<ServiceResult<Request>> ApproveAsync(int requestId, ApproveRequestDto dto);
}
