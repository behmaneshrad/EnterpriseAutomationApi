using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using Serilog;

// ==== EnterpriseAutomation namespaces ====
using EnterpriseAutomation.Api.Authorization;
using EnterpriseAutomation.Api.Middelware;
using EnterpriseAutomation.Api.Middelware.AuthorizeMIddelware;
using EnterpriseAutomation.Api.Security;

using EnterpriseAutomation.Application.Services;
using EnterpriseAutomation.Application.Services.Interfaces;

using EnterpriseAutomation.Infrastructure;
using EnterpriseAutomation.Infrastructure.Persistence;
using EnterpriseAutomation.Infrastructure.Repository;

// +++ اضافه برای Mongo (لایه Infrastructure) +++
using EnterpriseAutomation.Infrastructure.Mongo;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Application.Requests.Services; // << AddMongoWorkflowLogging extension

var builder = WebApplication.CreateBuilder(args);

// ==============================
// Keycloak Authentication (JWT)
// ==============================
var keycloakSettings = builder.Configuration.GetSection("KeycloakSettings").Get<KeycloakSettings>()
    ?? throw new Exception("KeycloakSettings section is missing in appsettings.json");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakSettings.AuthServerUrl}/realms/{keycloakSettings.Realm}";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudiences = new[] { "account", keycloakSettings.ClientId },
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                // userId از توکن
                var userId = context.Principal?.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                    claimsIdentity?.AddClaim(new Claim("userId", userId));

                // اگر userId نبود، از sub
                var sub = context.Principal?.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(sub) && claimsIdentity is not null && !claimsIdentity.HasClaim(c => c.Type == "userId"))
                    claimsIdentity.AddClaim(new Claim("userId", sub));

                // realm_access.roles
                var realmRoles = context.Principal?.FindAll("realm_access.roles");
                if (realmRoles is not null)
                    foreach (var role in realmRoles)
                        claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role.Value));

                // resource_access.{client}.roles
                var resourceRoles = context.Principal?.FindAll("resource_access.PartakCRMClient.roles");
                if (resourceRoles is not null)
                    foreach (var role in resourceRoles)
                        claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role.Value));

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                    context.Response.Headers.Add("Token-Expired", "true");
                return Task.CompletedTask;
            }
        };
    });

// ==============================
// Serilog
// ==============================
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

// ==============================
// Authorization
// ==============================
builder.Services.AddScoped<IAuthorizationHandler, AutoPermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

builder.Services.AddAuthorization(options =>
{
    // تنظیم پیام‌های پیش‌فرض
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();

    // ثبت Policy برای تشخیص خودکار دسترسی‌ها
    options.AddPolicy("AutoPermission", policy =>
        policy.Requirements.Add(new AutoPermissionRequirement()));
});


// ==============================
// CORS
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy.WithOrigins() // اگر Origin خاص دارید اینجا قرار دهید
              .AllowAnyHeader()
              .AllowAnyMethod();
        // .AllowCredentials();
    });
});

// ==============================
// Controllers / Swagger / HttpContext
// ==============================


if (builder.Configuration.GetValue<bool>("Swagger:Enabled", true))
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Enterprise API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] + your token"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                },
                Array.Empty<string>()
            }
        });
    });
}

// ==============================
// Elasticsearch
// ==============================
var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];
var connectionSettings = new ConnectionSettings(new Uri(elasticUri!))
    .DefaultIndex("workflow-logs")
    .BasicAuthentication("", ""); // مقادیر امن را در appsettings قرار دهید

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(connectionSettings));

// ==============================
// Application Services
// ==============================
builder.Services.AddHttpClient<KeycloakService>();
builder.Services.Configure<KeycloakSettings>(builder.Configuration.GetSection("KeycloakSettings"));

builder.Services.AddScoped<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowStepsService, WorkflowStepsService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();
builder.Services.AddScoped<ITestServiceMeet8, TestServiceMeet8>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();

// Generic Repository
builder.Services.AddScoped(typeof(EnterpriseAutomation.Application.IRepository.IRepository<>), typeof(Repository<>));

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<AppDbContextFactory>();

// Provider/Handler های Permission (در صورت نیاز)
builder.Services.AddScoped<PermissionPolicyProvider>();
builder.Services.AddScoped<PermissionHandler>();

// ==============================
// Dynamic Policies from DB
// ==============================

// ثبت Policyهای داینامیک بعد از ثبت همه سرویس‌ها
var dbPermissions = builder.Services.BuildServiceProvider()
    .CreateScope().ServiceProvider
    .GetRequiredService<IPermissionService>()
    .GetAllAsync().GetAwaiter().GetResult();

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in dbPermissions)
    {
        options.AddPolicy(permission.Name, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission.Name)));
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMongoWorkflowLogging(builder.Configuration);

// ==============================
// Build app
// ==============================
var app = builder.Build();

if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Swagger:Enabled", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List));
}

app.UseHttpsRedirection();
app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();

// پیام‌میانی سفارشی احراز مجوز (اگر لازم است)
//app.UseMiddleware<AuthorizeMessageMW>();

app.MapControllers();
app.Run();
