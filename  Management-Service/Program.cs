using ManagementService.Api.Middleware;
using ManagementService.Application;
using ManagementService.Infrastructure;
using Serilog;
using ManagementService.Api.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (integrate with Shared-Utilities logging infrastructure)
// Placeholder: Actual logging setup would come from Shared.Utilities library
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Management Service API", Version = "v1" });
});


// Add Application and Infrastructure layer services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); // For gRPC reflection

// Configure HttpClient with Polly for DataServiceApiClient and DeploymentOrchestrationClient
var pollyDefaultRetryCount = builder.Configuration.GetValue<int>("PollyConfigs:DefaultRetryCount", 3);
var pollyDefaultTimeoutSeconds = builder.Configuration.GetValue<int>("PollyConfigs:DefaultTimeoutSeconds", 30);

var retryPolicy = Polly.Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>() // Handle timeout as TaskCanceledException
    .WaitAndRetryAsync(pollyDefaultRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var timeoutPolicy = Polly.Policy.TimeoutAsync(TimeSpan.FromSeconds(pollyDefaultTimeoutSeconds));
var circuitBreakerPolicy = Polly.Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );

var policyWrap = Polly.Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);


builder.Services.AddHttpClient("DataServiceClient", client =>
{
    var dataServiceUrl = builder.Configuration["ServiceEndpoints:DataService"];
    if (string.IsNullOrEmpty(dataServiceUrl))
        throw new InvalidOperationException("DataService endpoint URL is not configured.");
    client.BaseAddress = new Uri(dataServiceUrl);
}).AddPolicyHandler(policyWrap);

builder.Services.AddHttpClient("DeploymentOrchestrationClient", client =>
{
    var deploymentServiceUrl = builder.Configuration["ServiceEndpoints:DeploymentOrchestrationService"];
    if (string.IsNullOrEmpty(deploymentServiceUrl))
        throw new InvalidOperationException("DeploymentOrchestrationService endpoint URL is not configured.");
    client.BaseAddress = new Uri(deploymentServiceUrl);
}).AddPolicyHandler(policyWrap);

// Add AuthN/AuthZ (Delegated to SecurityService/API Gateway)
// Placeholder: Actual implementation might involve JWT bearer token validation middleware
// configured to use the SecurityService for token validation.
// For now, basic setup for ASP.NET Core Identity or similar could be added.
// services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });
builder.Services.AddAuthentication(); // Add appropriate scheme like JWT
builder.Services.AddAuthorization();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ManagementService.Infrastructure.Persistence.ManagementDbContext>("Database");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Management Service API V1"));
    app.MapGrpcReflectionService(); // Enable gRPC reflection for development
}

// Custom Error Handling Middleware
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

// Add AuthN/AuthZ middleware - order matters
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ManagementGrpcApiService>(); // Map gRPC service

app.MapHealthChecks("/health");

app.MapGet("/", () => "Management Service is running. Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();