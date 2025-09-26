using EnterpriseAutomation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services.Interfaces
{
    public interface ITestServiceMeet8
    {
        public Task<WorkflowDefinition?>  Get(int WorkflowId);
    }
}
