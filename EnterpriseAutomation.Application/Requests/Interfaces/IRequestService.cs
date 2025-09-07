using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Requests.Interfaces
{
    public interface IRequestService
    {
        Task CreateRequestAsync(CreateRequestDto dto);
        Task<IEnumerable<Request>> GetAllRequestsAsync();
        Task<Request?> GetRequestByIdAsync(int requestId);
        Task<bool> SubmitRequestAsync(SubmitRequestDto dto);
        Task<IEnumerable<WorkflowStep>> GetWorkflowStepsAsync(int workflowDefinitionId);
        Task<ServiceResult<Request>> GetFilteredRequestsAsync(string? status, string? role, Guid? createdBy, int pageIndex, int pageSize); //گرفتن درخواست ها با فیلتر
        Task ApproveAsync(int requestId, bool isApproved, string? comment);
    }
}