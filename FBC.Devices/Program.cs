using FBC.Devices.Components;
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

builder.Services.AddHostedService<FBC.Devices.Services.DeviceStatusService>();
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
