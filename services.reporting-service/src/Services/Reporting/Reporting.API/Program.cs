using Hangfire;
using Reporting.Application;
using Reporting.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure structured logging with Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Reporting Service API",
        Version = "v1",
        Description = "API for generating, scheduling, and managing reports."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // In a real app, you might want this behind an auth policy
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();

// Add Serilog request logging
app.UseSerilogRequestLogging();

// In a real system, these would be configured properly.
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();