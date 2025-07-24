using EnterpriseAutomation.Application.Users.Interfaces;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
//using EnterpriseAutomation.Application.Users.Services;
using EnterpriseAutomation.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;
using EnterpriseAutomation.Application.Users.Services;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<KeycloakService>();
//builder.Services.AddScoped<KeycloakService>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();

// Gen Repository Service
builder.Services.AddScoped(typeof(IRepository<>),typeof(Repository<>));


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
        ValidAudience = "enterprise-api"
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed= context =>
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"OnAuthenticationFailed\"}");
        },
        OnChallenge= context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType= "application/json";
            return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"OnChallenge\"}");
        },
        OnForbidden= context =>
        { 
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"OnForbidden\"}");
        }
    };
});

//نیاز توکن از keycloak هستیم

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
app.Run();
