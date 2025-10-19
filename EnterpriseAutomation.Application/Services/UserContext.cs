using EnterpriseAutomation.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.Services
{
    public static class UserContext
    {
        private static readonly AsyncLocal<string> _currentKeycloakId = new AsyncLocal<string>();

        public static string CurrentKeycloakId
        {
            get => _currentKeycloakId.Value;
            set => _currentKeycloakId.Value = value;
        }
    }
}
