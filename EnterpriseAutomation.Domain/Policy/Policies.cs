
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Domain.Entities.Policy
{
    public static class Policies
    {
        public const string Accounts_GetRoles = "Accounts.GetRoles";          // admin
        public const string Accounts_GetUsers = "Accounts.GetUsers";          // admin

        public const string Requests_CreateRequest = "Requests.CreateRequest";     // user
        public const string Requests_GetAllRequests = "Requests.GetAllRequests";    // admin
        public const string Requests_GetRequestById = "Requests.GetRequestById";    // admin OR approver
        public const string Requests_GetWorkflowSteps = "Requests.GetWorkflowSteps";  // admin
        public const string Requests_GetFilteredRequests = "Requests.GetFilteredRequests";// admin OR approver

        public const string WorkflowDefinitions_Get = "WorkflowDefinitions.Get";    // admin
    }

}