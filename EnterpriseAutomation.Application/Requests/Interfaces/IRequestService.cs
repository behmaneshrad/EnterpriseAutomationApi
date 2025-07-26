using EnterpriseAutomation.Application.Requests.Models;
using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Requests.Interfaces
{
    public interface IRequestService
    {
        Task<int> CreateRequestAsync(CreateRequestDto dto);
        Task<List<Request>> GetAllRequestsAsync();
        Task<Request?> GetRequestByIdAsync(int requestId);
        Task<bool> SubmitRequestAsync(SubmitRequestDto dto);
    }
}
