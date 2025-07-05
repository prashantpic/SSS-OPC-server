using ManagementService.Api.GrpcServices;
using ManagementService.Application.Contracts.Messaging;
using ManagementService.Application.Contracts.Persistence;
using ManagementService.Infrastructure.Messaging;
using ManagementService.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Configure Logging ---
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// --- Add services to the DI container ---

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add Infrastructure Services
builder.Services.AddScoped<IOpcClientInstanceRepository, OpcClientInstanceRepository>();

// This is a dummy implementation as the real one is not in this file batch.
// In a real scenario, this would be in its own file in Infrastructure/Messaging.
builder.Services.AddSingleton<IConfigurationUpdatePublisher, DummyConfigurationUpdatePublisher>(); 

// Configure typed HttpClient for Data Service communication
builder.Services.AddHttpClient("DataServiceClient", client =>
{
    var dataServiceUrl = builder.Configuration["DataServiceUrl"];
    if (string.IsNullOrEmpty(dataServiceUrl))
    {
        throw new InvalidOperationException("DataServiceUrl is not configured in appsettings.json");
    }
    client.BaseAddress = new Uri(dataServiceUrl);
});

// Add Presentation Layer services
builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcSwagger();

// Add Swagger/OpenAPI for REST API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Management Service API", Version = "v1" });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri(builder.Configuration["DataServiceUrl"] + "/health"), "Data Service");


// --- Build the application ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Management Service API v1");
    });
}

// Add global exception handling middleware (a real implementation would be more robust)
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
        Log.Error(ex, "An unhandled exception has occurred");
    }
});


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapGrpcService<ClientLifecycleGrpcService>();
app.MapHealthChecks("/health");

app.Run();