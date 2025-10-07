using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services
{
    public class TestServiceMeet8 : ITestServiceMeet8
    {
        private readonly IRepository<WorkflowDefinition> _rep;
        public TestServiceMeet8(IRepository<WorkflowDefinition> rep)
        {
            _rep = rep;
        }
        public async Task<WorkflowDefinition?> Get(int WorkflowId)
        {
            var res =await _rep.GetFirstWithInclude(p=>p.WorkflowDefinitionId == WorkflowId,
                c=>c.Include(s=>s.Requests).Include(x=>x.WorkflowSteps),
                asNoTracking:true);

            return res;
        }

    }
}
