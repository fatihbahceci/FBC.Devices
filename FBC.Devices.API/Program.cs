using FBC.DBRepository;
using FBC.Devices.API.DBModels.Repository;
using FBC.Devices.DBModels;
using FBC.Mediator;
using Serilog;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog((context, configuration) =>
{
    //https://github.com/serilog/serilog-settings-configuration
    configuration.ReadFrom.Configuration(context.Configuration);
});

// It is important for enum serialization in swagger and API responses.
// For more inormation see: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293 (Enum string converter is not respected using .NET 6 minimal API) -FBC
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
//The code below is also required for properly send enum data as string on json serialization for Post/Put requests. -FBC
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info: new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "FBC.Devices.API",
        Version = "v1",
        Description = "API for managing devices."
    });
});
DB.MigrateDB();
builder.Services.AddDbContext<DB>();
builder.Services.RegisterRepositories(typeof(Program).Assembly);
builder.Services.AddMediator(typeof(Program).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMediatorEndpoints();
app.AddExceptionHandlerMiddleware();

app.Run();
