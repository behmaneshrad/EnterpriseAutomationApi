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


builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy.WithOrigins("FrontPolicy", "http://localhost:4000");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<KeycloakService>();

// Register and Request Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowStepsService, WorkflowStepsService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();

// Generic Repository Service
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier // این باعث می‌شود sub به NameIdentifier map شود
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

            // اضافه کردن sub claim به عنوان NameIdentifier اگر وجود نداشته باشد
            var subClaim = claimsIdentity?.FindFirst("sub");
            if (subClaim != null && !claimsIdentity.HasClaim(ClaimTypes.NameIdentifier, subClaim.Value))
            {
                claimsIdentity?.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }

            // اضافه کردن preferred_username به عنوان Name claim
            var usernameClaim = claimsIdentity?.FindFirst("preferred_username");
            if (usernameClaim != null && !claimsIdentity.HasClaim(ClaimTypes.Name, usernameClaim.Value))
            {
                claimsIdentity?.AddClaim(new Claim(ClaimTypes.Name, usernameClaim.Value));
            }

            // Extract roles from Keycloak token structure
            // Get realm_access roles
            var realmAccessClaim = claimsIdentity?.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                try
                {
                    var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
                    if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue) && !claimsIdentity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Handle JSON parsing error
                }
            }

            // Get resource_access roles for enterprise-api
            var resourceAccessClaim = claimsIdentity?.FindFirst("resource_access");
            if (resourceAccessClaim != null)
            {
                try
                {
                    var resourceAccess = JsonDocument.Parse(resourceAccessClaim.Value);
                    if (resourceAccess.RootElement.TryGetProperty("enterprise-api", out var enterpriseApi) &&
                        enterpriseApi.TryGetProperty("roles", out var resourceRoles))
                    {
                        foreach (var role in resourceRoles.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue) && !claimsIdentity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Handle JSON parsing error
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
app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();




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