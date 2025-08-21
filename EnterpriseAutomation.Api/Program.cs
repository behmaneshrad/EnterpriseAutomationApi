using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

// Security / AuthZ
using EnterpriseAutomation.Api.Security;

// DI Contracts
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

// Infra
using EnterpriseAutomation.Infrastructure.Persistence;
using EnterpriseAutomation.Infrastructure.Repository;
using EnterpriseAutomation.Infrastructure.Services;

// ASP.NET & EF & Swagger
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;
// for log
using Serilog;

// Options alias
using KeycloakOptions = EnterpriseAutomation.Infrastructure.Services.KeycloakOptions;

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

// ===== Authorization =====
// سیاست پیش‌فرض: همه اکشن‌ها نیاز به احراز هویت دارند
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // سیاست داینامیک HasAccess (کنترل دسترسی بر اساس مسیر/کنترلر از DB)
    options.AddPolicy("HasAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new HasAccessRequirement());
    });
});

// ===== Authorization Handlers =====
builder.Services.AddScoped<IAuthorizationHandler, HasAccessHandler>();

// اگر هم‌زمان PermissionRequirement/PermissionHandler را جای دیگری استفاده می‌کنی، این خط را نگه دار:
// builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// ===== JWT (Keycloak) =====
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]?.TrimEnd('/');
var realm = builder.Configuration["Keycloak:Realm"];
var audience = builder.Configuration["Keycloak:Audience"]; // مثلا "enterprise-api"

// نمایش PII فقط در Dev
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // آدرس OIDC (issuer)
    options.Authority = $"{keycloakAuthority}/realms/{realm}";
    options.RequireHttpsMetadata = false; // اگر Keycloak روی http است

    // جلوگیری از مپ قدیمی Claimها
    options.MapInboundClaims = false;

    // اعتبارسنجی دقیق
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,

        ValidateIssuer = true,
        ValidIssuer = $"{keycloakAuthority}/realms/{realm}",

        ValidateAudience = true,
        AudienceValidator = (audiences, securityToken, validationParameters) =>
        {
            var target = audience; // مثلا "enterprise-api"
            if (string.IsNullOrWhiteSpace(target)) return false;

            // 1) aud شامل target بود
            if (audiences != null && audiences.Contains(target, StringComparer.Ordinal))
                return true;

            // 2) در غیر این صورت resource_access را چک کن
            if (securityToken is JwtSecurityToken jwt)
            {
                var ra = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                if (!string.IsNullOrEmpty(ra))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(ra);
                        if (doc.RootElement.TryGetProperty(target, out _))
                            return true;
                    }
                    catch { /* ignore parse errors */ }
                }
            }
            return false;
        },

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),

        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var ci = context.Principal?.Identity as ClaimsIdentity;
            if (ci is null) return Task.CompletedTask;

            // sub -> NameIdentifier
            var sub = ci.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(sub) && !ci.HasClaim(ClaimTypes.NameIdentifier, sub))
                ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));

            // preferred_username -> Name
            var uname = ci.FindFirst("preferred_username")?.Value;
            if (!string.IsNullOrEmpty(uname) && !ci.HasClaim(ClaimTypes.Name, uname))
                ci.AddClaim(new Claim(ClaimTypes.Name, uname));

            // realm_access.roles
            var realmAccess = ci.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccess))
            {
                try
                {
                    using var doc = JsonDocument.Parse(realmAccess);
                    if (doc.RootElement.TryGetProperty("roles", out var rolesEl))
                    {
                        foreach (var r in rolesEl.EnumerateArray())
                        {
                            var rv = r.GetString();
                            if (!string.IsNullOrWhiteSpace(rv) && !ci.HasClaim(ClaimTypes.Role, rv))
                                ci.AddClaim(new Claim(ClaimTypes.Role, rv));
                        }
                    }
                }
                catch { /* ignore */ }
            }

            // resource_access.{audience}.roles
            var resourceAccess = ci.FindFirst("resource_access")?.Value;
            if (!string.IsNullOrEmpty(resourceAccess) && !string.IsNullOrEmpty(audience))
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccess);
                    if (doc.RootElement.TryGetProperty(audience, out var clientObj) &&
                        clientObj.TryGetProperty("roles", out var rr))
                    {
                        foreach (var r in rr.EnumerateArray())
                        {
                            var rv = r.GetString();
                            if (!string.IsNullOrWhiteSpace(rv) && !ci.HasClaim(ClaimTypes.Role, rv))
                                ci.AddClaim(new Claim(ClaimTypes.Role, rv));
                        }
                    }
                }
                catch { /* ignore */ }
            }

            return Task.CompletedTask;
        },

        // 401 (Invalid Token)
        OnAuthenticationFailed = async ctx =>
        {
            ctx.NoResult();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json; charset=utf-8";

            var msg = new
            {
                error = "Unauthorized",
                message = "توکن معتبر نیست.",
                detail = ctx.Exception?.Message
            };

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(msg));
        },

        // 401 Challenge
        OnChallenge = async ctx =>
        {
            if (!ctx.Response.HasStarted)
            {
                ctx.Response.ContentType = "application/json; charset=utf-8";
                var payload = "{\"error\":\"Unauthorized\",\"message\":\"دسترسی غیرمجاز - لطفاً وارد شوید.\"}";
                await ctx.Response.WriteAsync(payload);
            }
            ctx.HandleResponse();
        },

        // 403 Forbidden
        OnForbidden = async ctx =>
        {
            ctx.Response.ContentType = "application/json; charset=utf-8";
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

// ===== Pipeline =====
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
