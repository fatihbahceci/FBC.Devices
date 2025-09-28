using FBC.Devices.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebApplication.CreateBuilder(args);
FBC.Devices.DBModels.DB.MigrateDB();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorComponents();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<FBC.Devices.DBModels.DB>();
builder.Services.AddScoped<FBC.Devices.DBModels.DeviceDB>();

//Authentication
builder.Services.AddScoped<AuthenticationStateProvider, FBC.Devices.Services.FBCAuthenticationStateProvider>();
builder.Services.AddScoped<FBC.Devices.Services.FBCAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthorizationHandler, FBC.Devices.Services.FBCAuthorizationHandler>();
builder.Services.AddAuthorizationCore();
/*
// For: InvalidOperationException: Unable to find the required 'IAuthenticationService' service. Please add all the required services by calling 'IServiceCollection.AddAuthentication' in the application startup code.
builder.Services.AddAuthentication("FBCCustomScheme") // Dummy auth
    .AddScheme<AuthenticationSchemeOptions, FBC.Devices.Services.DummyAuthenticationHandler>("FBCCustomScheme", null);
// Middleware for authentication (for IAuthenticationService exception)
builder.Services.AddAuthorization(); // Normal middleware auth
*/

builder.Services.AddHostedService<FBC.Devices.Services.DeviceStatusService>();
builder.Services.AddHostedService<FBC.Devices.Services.DeviceSearchDataService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
