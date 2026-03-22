using System.Text;
using FBC.DBRepository;
using FBC.Devices.API;
using FBC.Devices.API.Data;
using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Services;
using FBC.Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database
AppDbContext.MigrateDB();
builder.Services.AddDbContext<AppDbContext>();

// Current user provider for audit tracking and role-based access control
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

// Repositories - register both interface and concrete type
builder.Services.RegisterRepositories(typeof(Program).Assembly);
builder.Services.AddScoped<AddrTypeRepository>();
builder.Services.AddScoped<DeviceTypeRepository>();
builder.Services.AddScoped<DeviceGroupRepository>();
builder.Services.AddScoped<DeviceRepository>();
builder.Services.AddScoped<DeviceAddrRepository>();
builder.Services.AddScoped<UserRepository>();

// FBC.Mediator
builder.Services.AddMediator(typeof(Program).Assembly);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Authorization with SysAdmin bypass - define a named policy per role
var authBuilder = builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// Register each role as a named policy so RequireAuthorization("RoleName") works
foreach (var role in Constants.UserRoles.AllRolesButSysAdmin.Append(Constants.UserRoles.SysAdmin))
{
    authBuilder.AddPolicy(role, policy => policy.RequireRole(role));
}

builder.Services.AddSingleton<IAuthorizationHandler, SysAdminAuthorizationHandler>();

// Background Services
builder.Services.AddHostedService<DeviceStatusService>();
builder.Services.AddHostedService<DeviceSearchDataService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// JSON camelCase
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opt.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// FBC.Mediator endpoints
app.UseMediatorEndpoints();

// SPA fallback - anonymous so login page is accessible
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

// SysAdmin role bypasses all authorization checks
public class SysAdminAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RolesAuthorizationRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (context.User.IsInRole(Constants.UserRoles.SysAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        foreach (var role in requirement.AllowedRoles)
        {
            if (context.User.IsInRole(role))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
