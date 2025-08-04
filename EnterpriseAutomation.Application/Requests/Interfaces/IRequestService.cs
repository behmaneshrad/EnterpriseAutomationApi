using EnterpriseAutomation.Application.Requests.Models;
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
        Task<IEnumerable<Request>> GetFilteredRequestsAsync(RequestStatus? status, string? role, int? createdBy); //گرفتن درخواست ها با فیلتر

    }
}