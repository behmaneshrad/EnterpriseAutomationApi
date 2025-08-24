using EnterpriseAutomation.Api.Middelware.AuthorizeMIddelware;
using EnterpriseAutomation.Api.Security;
using EnterpriseAutomation.Application.IRepository;
// Request Services
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Services;
using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Application.Users.Services;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Services;
using EnterpriseAutomation.Application.WorkflowSteps.Interfaces;
using EnterpriseAutomation.Application.WorkflowSteps.Services;
using EnterpriseAutomation.Infrastructure.Persistence;
using EnterpriseAutomation.Infrastructure.Repository;
using EnterpriseAutomation.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;
// for log
using Serilog;
using EnterpriseAutomation.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<KeycloakService>();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowStepsService, WorkflowStepsService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();

// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<AppDbContextFactory>();

// (اگر از Provider/Handler سفارشی استفاده می‌کنی نگه‌شان دار)
builder.Services.AddScoped<PermissionPolicyProvider>();
builder.Services.AddScoped<PermissionHandler>();

// ===== Authorization Policies (مطابق جدول) =====
builder.Services.AddAuthorization(options =>
{
    // AccountsController
    options.AddPolicy("Accounts.GetRoles", p => p.RequireRole("admin"));
    options.AddPolicy("Accounts.GetUsers", p => p.RequireRole("admin"));

    // RequestsController
    options.AddPolicy("Requests.CreateRequest", p => p.RequireRole("user"));
    options.AddPolicy("Requests.GetAllRequests", p => p.RequireRole("admin"));
    options.AddPolicy("Requests.GetRequestById", p => p.RequireRole("admin", "approver")); // OR
    options.AddPolicy("Requests.GetWorkflowSteps", p => p.RequireRole("admin"));
    options.AddPolicy("Requests.GetFilteredRequests", p => p.RequireRole("admin", "approver")); // OR

    // WorkflowDefinitionsController
    options.AddPolicy("WorkflowDefinitions.Get", p => p.RequireRole("admin"));

    // همه‌ی اکشن‌ها به‌صورت پیش‌فرض نیاز به احراز هویت دارند
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ===== JWT (Keycloak) =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "http://localhost:8080/realms/EnterpriseRealm";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = "enterprise-api",
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

            // sub -> NameIdentifier
            var subClaim = claimsIdentity?.FindFirst("sub");
            if (subClaim != null && !claimsIdentity.HasClaim(ClaimTypes.NameIdentifier, subClaim.Value))
                claimsIdentity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));

            // preferred_username -> Name
            var usernameClaim = claimsIdentity?.FindFirst("preferred_username");
            if (usernameClaim != null && !claimsIdentity.HasClaim(ClaimTypes.Name, usernameClaim.Value))
                claimsIdentity?.AddClaim(new Claim(ClaimTypes.Name, usernameClaim.Value));

            // realm_access.roles -> Role
            var realmAccessClaim = claimsIdentity?.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleValue) &&
                                !claimsIdentity!.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity!.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            // resource_access.enterprise-api.roles -> Role
            var resourceAccessClaim = claimsIdentity?.FindFirst("resource_access");
            if (resourceAccessClaim != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccessClaim.Value);
                    if (doc.RootElement.TryGetProperty("enterprise-api", out var enterpriseApi) &&
                        enterpriseApi.TryGetProperty("roles", out var resourceRoles))
                    {
                        foreach (var role in resourceRoles.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleValue) &&
                                !claimsIdentity!.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity!.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            return Task.CompletedTask;
        }
    };
});

// Swagger
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

app.UseHttpsRedirection();

app.UseCors("FrontPolicy");

app.UseAuthentication();
app.UseAuthorization();

// پیام‌های خطای 401/403 یکدست
app.Use(async (context, next) =>
{
    await next();
    if (!context.Response.HasStarted)
    {
        if (context.Response.StatusCode == 401)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"دسترسی غیرمجاز - لطفاً وارد شوید.\"}");
        }
        else if (context.Response.StatusCode == 403)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"شما مجوز دسترسی به این بخش را ندارید.\"}");
        }
    }
});

// اگر لازم است پیام‌ میانی خودت اجرا شود
app.UseMiddleware<AuthorizeMessageMW>();

app.MapControllers();

app.Run();