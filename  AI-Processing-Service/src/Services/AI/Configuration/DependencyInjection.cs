using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR; // Assuming MediatR is used for CQRS

// Domain Layer using statements (interfaces)
using AIService.Domain.Interfaces;
using AIService.Domain.Services;

// Application Layer using statements (interfaces and implementations)
using AIService.Application.Interfaces;
using AIService.Application.Services;
// --- Add specific command/handler namespaces if MediatR scans by assembly ---
// e.g., using AIService.Application.Features.PredictiveMaintenance.Commands;

// Infrastructure Layer using statements (implementations)
using AIService.Infrastructure.AI.Common;
using AIService.Infrastructure.AI.ONNX;
using AIService.Infrastructure.AI.TensorFlow;
using AIService.Infrastructure.AI.MLNet;
using AIService.Infrastructure.NLP.SpaCy;
using AIService.Infrastructure.NLP.AzureCognitive;
using AIService.Infrastructure.MLOps.MLflow;
using AIService.Infrastructure.Persistence;
using AIService.Infrastructure.Clients;
using AIService.Infrastructure.Utils;


namespace AIService.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly()); // Or specify assemblies containing profiles

            // Add MediatR
            // Ensure MediatR is added to the .csproj file: <PackageReference Include="MediatR" Version="12.x.x" />
            // <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.x.x" /> is usually not needed for MediatR 12+
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


            // Register Application Services
            services.AddScoped<IModelManagementAppService, ModelManagementAppService>();
            services.AddScoped<IEdgeDeploymentAppService, EdgeDeploymentAppService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Domain Services
            services.AddScoped<ModelExecutionService>(); // Concrete class, not interface, if it orchestrates multiple IModelExecutionEngine
            services.AddScoped<NlpOrchestrationService>(); // Concrete class, for orchestrating INlpProvider

            // Register AI Execution Engines (as a collection)
            services.AddScoped<IModelExecutionEngine, OnnxExecutionEngine>();
            services.AddScoped<IModelExecutionEngine, TensorFlowExecutionEngine>();
            services.AddScoped<IModelExecutionEngine, MLNetExecutionEngine>();

            // Register NLP Providers (as a collection, NlpOrchestrationService will select one based on config)
            services.AddScoped<INlpProvider, SpaCyNlpProvider>();
            services.AddScoped<INlpProvider, AzureCognitiveServicesNlpProvider>();

            // Register MLOps Client (conditional registration or factory could be used here based on MLOpsOptions.PlatformType)
            // For simplicity, registering one for now.
            services.AddScoped<IMlLopsClient, MlflowClientAdapter>();
            // Example for AzureML (requires its own adapter)
            // services.AddScoped<IMlLopsClient, AzureMLClientAdapter>();


            // Register Repositories and Clients
            services.AddScoped<IModelRepository, ModelRepository>();

            // Configure and register DataServiceClient (gRPC client for REPO-DATA-SERVICE)
            // The URL should come from configuration
            var dataServiceUrl = configuration.GetValue<string>("DataServiceGrpcUrl");
            if (string.IsNullOrEmpty(dataServiceUrl))
            {
                // Fallback or throw exception if not configured, for development purposes
                dataServiceUrl = "http://localhost:50051"; // Example default, should be in appsettings
                // Or throw new InvalidOperationException("DataServiceGrpcUrl is not configured.");
            }
            services.AddGrpcClient<DataServiceClient>(options => // DataServiceClient is the generated client class
            {
                options.Address = new System.Uri(dataServiceUrl);
            });


            // Register Utilities
            services.AddSingleton<ModelFileLoader>(); // Singleton if it caches, Scoped otherwise
            services.AddSingleton<ImageProcessingUtils>(); // Utilities are often stateless singletons

            return services;
        }
    }
}