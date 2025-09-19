using EnterpriseAutomation.Api.Authorization;
using EnterpriseAutomation.Api.Middelware;
using EnterpriseAutomation.Api.Middelware.AuthorizeMIddelware;
using EnterpriseAutomation.Api.Security;
using EnterpriseAutomation.Application.Services;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure;
using EnterpriseAutomation.Infrastructure.Persistence;
using EnterpriseAutomation.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using Serilog;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Keycloak Authentication
var keycloakSettings = builder.Configuration.GetSection("KeycloakSettings").Get<KeycloakSettings>();

if (keycloakSettings == null)
{
    throw new Exception("KeycloakSettings section is missing in appsettings.json");
}

// ===== JWT (Keycloak) =====
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakSettings.AuthServerUrl}/realms/{keycloakSettings.Realm}";
        options.RequireHttpsMetadata = true;

        //  چند Audience مجاز
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudiences = new[] { "account", keycloakSettings.ClientId }, // هر دو معتبر
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                //  اضافه کردن userId از توکن
                var userId = context.Principal.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    claimsIdentity.AddClaim(new Claim("userId", userId));
                }

                //  اگر userId نبود، از sub استفاده می‌کنیم
                var sub = context.Principal.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(sub) && !claimsIdentity.HasClaim(c => c.Type == "userId"))
                {
                    claimsIdentity.AddClaim(new Claim("userId", sub));
                }

                //  مپ کردن نقش‌ها از realm_access.roles
                var realmRoles = context.Principal.FindAll("realm_access.roles");
                foreach (var role in realmRoles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
                }

                //  مپ کردن نقش‌ها از resource_access.{client}.roles
                var resourceRoles = context.Principal.FindAll("resource_access.PartakCRMClient.roles");
                foreach (var role in resourceRoles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

#region AddAuthentication
//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.Authority = $"{keycloakSettings.AuthServerUrl}/realms/{keycloakSettings.Realm}";
//        options.RequireHttpsMetadata = true;

//        //  چند Audience مجاز
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidAudiences = new[] { "account", keycloakSettings.ClientId }, // هر دو معتبر
//            NameClaimType = "preferred_username",
//            RoleClaimType = ClaimTypes.Role
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnTokenValidated = context =>
//            {
//                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

//                //  اضافه کردن userId از توکن
//                var userId = context.Principal.FindFirst("userId")?.Value;
//                if (!string.IsNullOrEmpty(userId))
//                {
//                    claimsIdentity.AddClaim(new Claim("userId", userId));
//                }

//                //  اگر userId نبود، از sub استفاده می‌کنیم
//                var sub = context.Principal.FindFirst("sub")?.Value;
//                if (!string.IsNullOrEmpty(sub) && !claimsIdentity.HasClaim(c => c.Type == "userId"))
//                {
//                    claimsIdentity.AddClaim(new Claim("userId", sub));
//                }

//                //  مپ کردن نقش‌ها از realm_access.roles
//                var realmRoles = context.Principal.FindAll("realm_access.roles");
//                foreach (var role in realmRoles)
//                {
//                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
//                }

//                //  مپ کردن نقش‌ها از resource_access.{client}.roles
//                var resourceRoles = context.Principal.FindAll("resource_access.PartakCRMClient.roles");
//                foreach (var role in resourceRoles)
//                {
//                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
//                }

//                return Task.CompletedTask;
//            },
//            OnAuthenticationFailed = context =>
//            {
//                if (context.Exception is SecurityTokenExpiredException)
//                {
//                    context.Response.Headers.Add("Token-Expired", "true");
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });
#endregion

// Elasticsearch configuration
var elasticUri = builder.Configuration["ElasticConfiguration:Uri"];
ConnectionSettings settings = new ConnectionSettings(new Uri(elasticUri))
    .DefaultIndex("workflow-logs")
    .BasicAuthentication("", "");

builder.Host.UseSerilog((ctx, Configuration) =>
{
    Configuration.ReadFrom.Configuration(ctx.Configuration);
});

// Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, AutoPermissionAuthorizationHandler>();

// ثبت مدیریت کننده سفارشی خطاهای احراز هویت
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy.WithOrigins()
              .AllowAnyHeader()
              .AllowAnyMethod();
        //.AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();


// Register KeycloakService with HttpClient
builder.Services.AddHttpClient<KeycloakService>();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowStepsService, WorkflowStepsService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.Configure<KeycloakSettings>(builder.Configuration.GetSection("KeycloakSettings"));
builder.Services.AddScoped<IKeycloakService, KeycloakService>();



//ثبت Policyهای داینامیک بعد از ثبت همه سرویس‌ها
//var dbPermissions = builder.Services.BuildServiceProvider()
//    .CreateScope().ServiceProvider
//    .GetRequiredService<IPermissionService>()
//    .GetAllAsync().GetAwaiter().GetResult();

//builder.Services.AddAuthorization(options =>
//{
//    foreach (var permission in dbPermissions)
//    {
//        options.AddPolicy(permission.Name, policy =>
//            policy.Requirements.Add(new PermissionRequirement(permission.Name)));
//    }
//});

// Register Elasticsearch client
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));
// Logger for workflow actions
//builder.Services.AddSingleton<IElasticWorkflowIndexService, ElasticWorkflowIndexService>();
//builder.Services.AddScoped<IWorkflowLogService, WorkflowLogService>();
// Generic Repository
builder.Services.AddScoped(typeof(EnterpriseAutomation.Application.IRepository.IRepository<>), typeof(Repository<>));

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<AppDbContextFactory>();

// (اگر از Provider/Handler سفارشی استفاده می‌کنی نگه‌شان دار)
builder.Services.AddScoped<PermissionPolicyProvider>();
builder.Services.AddScoped<PermissionHandler>();




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

// ثبت مدیریت کننده سفارشی خطاهای احراز هویت
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

var app = builder.Build();

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
    var dbPermissions = permissionService.GetAllAsync().GetAwaiter().GetResult();

    builder.Services.AddAuthorization(options =>
    {
        foreach (var permission in dbPermissions)
        {
            options.AddPolicy(permission.Name, policy =>
                policy.Requirements.Add(item: new PermissionRequirement(permission.Name)));
        }
    });
}

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



// اگر لازم است پیام‌ میانی خودت اجرا شود
app.UseMiddleware<AuthorizeMessageMW>();

app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var workflowIndexService = scope.ServiceProvider.GetRequiredService<IElasticWorkflowIndexService>();
//    await workflowIndexService.EnsureWorkflowIndexExistsAsync();
//}
app.Run();