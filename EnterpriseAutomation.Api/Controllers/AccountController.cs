using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EnterpriseAutomation.Application.Externals;
using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using EnterpriseAutomation.Application.Models.Users;
using EnterpriseAutomation.Application.Services.Interfaces;

namespace EnterpriseAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IKeycloakService _keycloakService;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _context; // Add DbContext dependency

        public AccountController(IKeycloakService keycloakService, ILogger<AccountController> logger, AppDbContext context)
        {
            _keycloakService = keycloakService;
            _logger = logger;
            _context = context; // Inject DbContext
        }       
    }
}