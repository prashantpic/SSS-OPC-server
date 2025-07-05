using AiService.Application;
using AiService.Application.Interfaces.Infrastructure;
using AiService.Domain.Interfaces.Repositories;
using AiService.Infrastructure.ExternalServices.DataService;
using AiService.Infrastructure.MachineLearning.Onnx;
using AiService.Infrastructure.Persistence;
using AiService.Infrastructure.Persistence.Repositories;
using AiService.Infrastructure.Storage;
using Azure.Storage.Blobs;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;

namespace AiService.Api.Extensions;

/// <summary>
/// Sets up all dependency injection registrations for the AI service.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Centralizes and organizes the registration of services from the Application layer in the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The same IServiceCollection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
        return services;
    }

    /// <summary>
    /// Centralizes and organizes the registration of services from the Infrastructure layer in the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The application configuration for accessing settings.</param>
    /// <returns>The same IServiceCollection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        var connectionString = configuration.GetConnectionString("AiServiceDb");
        services.AddDbContext<AiServiceDbContext>(options =>
            options.UseNpgsql(connectionString));
            
        services.AddScoped<IAiModelRepository, AiModelRepository>();

        // Storage
        services.AddSingleton(x => new BlobServiceClient(configuration["Storage:BlobStorageConnectionString"]));
        services.AddScoped<IModelArtifactStorage, BlobModelArtifactStorage>();

        // Machine Learning Engines
        services.AddMemoryCache();
        services.AddScoped<IPredictionEngine, OnnxPredictionEngine>();
        // services.AddScoped<IAnomalyDetectionEngine, OnnxAnomalyDetectionEngine>(); // Assuming a similar implementation exists

        // External Service Clients
        services.AddGrpcClient<IDataServiceClient, DataServiceClient>(configuration);

        // services.AddScoped<INlpServiceProvider, AzureNlpServiceProvider>(); // Assuming implementation exists
        // services.AddScoped<IMlopsPlatformClient, MlopsPlatformClient>(); // Assuming implementation exists
        
        return services;
    }

    private static void AddGrpcClient<TInterface, TImplementation>(this IServiceCollection services, IConfiguration configuration)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var serviceUrl = configuration["ExternalServices:DataServiceUrl"];
        if (string.IsNullOrEmpty(serviceUrl))
        {
            // Don't register if the URL isn't configured.
            // Or, register a null/mock implementation.
            return;
        }

        services.AddSingleton(provider =>
        {
            var channelOptions = new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                }
            };
            return GrpcChannel.ForAddress(serviceUrl, channelOptions);
        });

        services.AddScoped<TInterface, TImplementation>();
    }
}