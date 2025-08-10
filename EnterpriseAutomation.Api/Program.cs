using EnterpriseAutomation.Api.Security;
using EnterpriseAutomation.Application.IRepository;
// Request Services
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Services;
using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Application.Users.Services;
using EnterpriseAutomation.Application.WorkflowDefinitions.Interfaces;
using EnterpriseAutomation.Application.WorkflowDefinitions.Services;
using EnterpriseAutomation.Infrastructure.Persistence;
using EnterpriseAutomation.Infrastructure.Repository;
using EnterpriseAutomation.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<KeycloakService>();

// Register and Request Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();

// Generic Repository Service
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// JWT Keycloak
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
        RoleClaimType = ClaimTypes.Role, // This is important for role mapping
        NameClaimType = "preferred_username" // Common for Keycloak
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Extract roles from Keycloak token structure
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

            // Get realm_access roles
            var realmAccessClaim = claimsIdentity?.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }
            }

            // Get resource_access roles for enterprise-api
            var resourceAccessClaim = claimsIdentity?.FindFirst("resource_access");
            if (resourceAccessClaim != null)
            {
                var resourceAccess = JsonDocument.Parse(resourceAccessClaim.Value);
                if (resourceAccess.RootElement.TryGetProperty("enterprise-api", out var enterpriseApi) &&
                    enterpriseApi.TryGetProperty("roles", out var resourceRoles))
                {
                    foreach (var role in resourceRoles.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            context.NoResult();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"توکن نامعتبر یا منقضی شده است.\"}");
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // جلوگیری از اجرای پیش‌فرض
            context.HandleResponse();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"ابتدا وارد سیستم شوید.\"}");
            }

            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"شما به این بخش دسترسی ندارید.\"}");
            }

            return Task.CompletedTask;
        }
    };
});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
//    options.AddPolicy("Employee", policy => policy.RequireRole("employee", "admin"));
//    options.AddPolicy("Approver", policy => policy.RequireRole("approver", "admin"));
//    options.AddPolicy("HR", policy => policy.RequireRole("hr", "admin"));
//    options.AddPolicy("Finance", policy => policy.RequireRole("finance", "admin"));
//    // Note: RequireUserName is not a standard method, you might want to use RequireRole instead
//    // options.AddPolicy("User", policy => policy.RequireRole("user"));
//});

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
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Handle 401/403 responses
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


app.Run();