using System.Security.Claims;
using System.Text.Json;
using EnterpriseAutomation.Api.Security;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Permissions.Interfaces;
using EnterpriseAutomation.Application.Permissions.Services;
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
using KeycloakOptions = EnterpriseAutomation.Infrastructure.Options.KeycloakOptions;

var builder = WebApplication.CreateBuilder(args);

// ===== Options =====
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:FrontOrigin"] ?? "http://localhost:4000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<KeycloakService>();

// ===== Application services =====
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IWorkflowStepsService, WorkflowStepsService>();
builder.Services.AddScoped<IWorkflowDefinitionsService, WorkflowDefinitionService>();

// Permission services
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ===== EF Core =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ===== Dynamic Policy =====
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// ===== Authorization (Fallback: همه اکشن‌ها احراز هویت) =====
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ===== JWT (Keycloak) =====
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]?.TrimEnd('/');
var realm = builder.Configuration["Keycloak:Realm"];
var audience = builder.Configuration["Keycloak:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = $"{keycloakAuthority}/realms/{realm}";
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    // استخراج Roleها از realm_access و resource_access
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity is null)
                return Task.CompletedTask;

            // sub -> NameIdentifier
            var subClaim = claimsIdentity.FindFirst("sub");
            if (subClaim != null && !claimsIdentity.HasClaim(ClaimTypes.NameIdentifier, subClaim.Value))
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));

            // preferred_username -> Name
            var usernameClaim = claimsIdentity.FindFirst("preferred_username");
            if (usernameClaim != null && !claimsIdentity.HasClaim(ClaimTypes.Name, usernameClaim.Value))
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, usernameClaim.Value));

            // realm_access.roles
            var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
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
                                !claimsIdentity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            // resource_access.{audience}.roles
            var resourceAccessClaim = claimsIdentity.FindFirst("resource_access");
            if (resourceAccessClaim != null && !string.IsNullOrEmpty(audience))
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccessClaim.Value);
                    if (doc.RootElement.TryGetProperty(audience, out var clientObj) &&
                        clientObj.TryGetProperty("roles", out var resourceRoles))
                    {
                        foreach (var role in resourceRoles.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleValue) &&
                                !claimsIdentity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            return Task.CompletedTask;
        },
        OnChallenge = async ctx =>
        {
            // 401 سفارشی
            if (!ctx.Response.HasStarted)
            {
                ctx.Response.ContentType = "application/json";
                var payload = "{\"error\":\"Unauthorized\",\"message\":\"دسترسی غیرمجاز - لطفاً وارد شوید.\"}";
                await ctx.Response.WriteAsync(payload);
            }
            ctx.HandleResponse(); // جلوگیری از پاسخ پیش‌فرض
        },
        OnForbidden = async ctx =>
        {
            // 403 سفارشی
            ctx.Response.ContentType = "application/json";
            var payload = "{\"error\":\"Forbidden\",\"message\":\"شما مجوز دسترسی به این بخش را ندارید.\"}";
            await ctx.Response.WriteAsync(payload);
        }
    };
});

// ===== Swagger =====
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

// ===== Middleware pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List));
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("FrontPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
