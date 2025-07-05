using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Reporting.Application.Contracts.Generation;
using Reporting.Application.Contracts.Infrastructure;
using Reporting.Application.Contracts.Services;
using Reporting.Infrastructure.Persistence;
using Reporting.Infrastructure.Persistence.Repositories;
using Reporting.Infrastructure.Services.FileStorage;
using Reporting.Infrastructure.Services.Generation;
using Reporting.Infrastructure.Services.Http;
using Reporting.Infrastructure.Scheduling;

namespace Reporting.Infrastructure;

/// <summary>
/// A setup class that encapsulates the dependency injection configuration for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all services defined in the Infrastructure layer to the IServiceCollection.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ReportingDb")));

        services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
        services.AddScoped<IGeneratedReportRepository, GeneratedReportRepository>();
        
        // Report Generation
        services.AddSingleton<IReportGenerator, PdfReportGenerator>();
        services.AddSingleton<IReportGenerator, ExcelReportGenerator>();
        services.AddSingleton<IReportGeneratorFactory, ReportGeneratorFactory>();

        // File Storage
        // Use a singleton for local storage to avoid re-reading config, but scoped/transient is also fine.
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        
        // Scheduling (Hangfire)
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("ReportingDb"))));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5; // Example: Configure worker count
        });
        
        // HTTP Clients for Inter-service Communication
        AddHttpClients(services, configuration);

        return services;
    }
    
    private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        // Define a retry policy for transient HTTP errors
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5xx, and 408
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff

        services.AddHttpClient<IAiServiceClient, AiServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:AiServiceUrl"] 
                                         ?? throw new InvalidOperationException("AI Service URL is not configured."));
        }).AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDataServiceClient, DataServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:DataServiceUrl"] 
                                         ?? throw new InvalidOperationException("Data Service URL is not configured."));
        }).AddPolicyHandler(retryPolicy);
    }
}