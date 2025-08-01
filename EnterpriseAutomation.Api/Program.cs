﻿using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EnterpriseAutomation.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;
using EnterpriseAutomation.Application.Users.Services;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Infrastructure.Repository;
// Request Services
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Application.Requests.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<KeycloakService>();


// Register and Request Services

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestService, RequestService>();

// Generic Repository Service
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        RoleClaimType = "roles"
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"توکن نامعتبر یا منقضی شده است.\"}");
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"ابتدا وارد سیستم شوید.\"}");
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"شما به این بخش دسترسی ندارید.\"}");
        }
    };

});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));

    options.AddPolicy("Employee", policy => policy.RequireRole("employee", "admin"));

    options.AddPolicy("Approver", policy => policy.RequireRole("approver", "admin"));

    options.AddPolicy("HR", policy => policy.RequireRole("hr", "admin"));

    options.AddPolicy("Finance", policy => policy.RequireRole("finance", "admin"));

    options.AddPolicy("User", policy => policy.RequireUserName("user"));

    options.AddPolicy("Employee", policy => policy.RequireRole("employee", "admin"));

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
//  401 ,403 errors
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"دسترسی غیرمجاز - لطفاً وارد شوید.\"}");
    }
    else if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"شما مجوز دسترسی به این بخش را ندارید.\"}");
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();