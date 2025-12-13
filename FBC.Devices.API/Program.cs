using FBC.Devices.API.DBModels.Repository;
using FBC.Mediator;
using FBC.Devices.DBModels;
using Serilog;
using FBC.DBRepository;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog((context, configuration) =>
{
    //https://github.com/serilog/serilog-settings-configuration
    configuration.ReadFrom.Configuration(context.Configuration);
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
