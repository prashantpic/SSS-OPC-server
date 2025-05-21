using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

// Assuming these namespaces will exist based on the SDS and file structure
using AIService.Application.Interfaces;
using AIService.Application.Services;
using AIService.Domain.Interfaces;
using AIService.Domain.Services;
using AIService.Infrastructure.AI.Common;
using AIService.Infrastructure.AI.Execution.ONNX;
using AIService.Infrastructure.AI.Execution.MLNet;
using AIService.Infrastructure.AI.Execution.TensorFlow;
using AIService.Infrastructure.Nlp.SpaCy;
using AIService.Infrastructure.Nlp.AzureCognitive;
using AIService.Infrastructure.MLOps.MLflow;
using AIService.Infrastructure.Persistence;
using AIService.Infrastructure.Clients;
// Api.Mappers would contain AutoMapper profiles
// Api.Controllers would contain controllers
// Api.GrpcServices would contain gRPC services
// Application.*.Commands and Application.*.Handlers for MediatR

namespace AIService.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure strongly-typed options
            services.Configure<ModelOptions>(configuration.GetSection("ModelOptions"));
            services.Configure<NlpProviderOptions>(configuration.GetSection("NlpProviderOptions"));
            services.Configure<MLOpsOptions>(configuration.GetSection("MLOpsOptions"));
            // REQ-7-006: Assuming DataServiceClient configuration is handled directly or via another Options class
            // services.Configure<DataServiceOptions>(configuration.GetSection("DataServiceOptions"));


            // Register AutoMapper (scans for profiles in the current assembly)
            // Ensure AutoMapper profiles are defined in e.g. AIService.Api.Mappers
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register MediatR (scans for handlers in the current assembly)
            // Ensure Commands and Handlers are defined in e.g. AIService.Application.*
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Application Layer Services
            services.AddScoped<IModelManagementAppService, ModelManagementAppService>(); // REQ-7-004, REQ-7-005, REQ-7-010
            services.AddScoped<IEdgeDeploymentAppService, EdgeDeploymentAppService>();   // REQ-8-001

            // Domain Layer Services
            services.AddScoped<ModelExecutionService>(); // REQ-7-003
            services.AddScoped<NlpOrchestrationService>(); // REQ-7-013, REQ-7-014, REQ-7-015, REQ-7-016

            // Infrastructure Layer - AI Execution Engines
            // These are registered as IEnumerable<IModelExecutionEngine> so ModelExecutionService can select the appropriate one.
            services.AddSingleton<IModelExecutionEngine, OnnxExecutionEngine>();         // REQ-7-003
            services.AddSingleton<IModelExecutionEngine, TensorFlowExecutionEngine>(); // For TensorFlow/TFLite models
            services.AddSingleton<IModelExecutionEngine, MLNetExecutionEngine>();       // For ML.NET models

            // Infrastructure Layer - NLP Providers
            // These are registered as IEnumerable<INlpProvider> so NlpOrchestrationService can select based on config.
            services.AddScoped<INlpProvider, SpaCyNlpProvider>();               // REQ-7-014
            services.AddScoped<INlpProvider, AzureCognitiveServicesNlpProvider>(); // REQ-7-014

            // Infrastructure Layer - MLOps Client
            // Select one MLOps client, potentially based on configuration.
            // For now, registering MLflow as an example.
            services.AddScoped<IMlLopsClient, MlflowClientAdapter>(); // REQ-7-004, REQ-7-010

            // Infrastructure Layer - Persistence
            services.AddScoped<IModelRepository, ModelRepository>(); // REQ-7-006, REQ-DLP-024 (interacts with DataServiceClient)

            // Infrastructure Layer - Clients
            // Assuming DataServiceClient is a gRPC client.
            // Its registration might involve AddGrpcClient and specific configuration.
            services.AddScoped<DataServiceClient>(); // REQ-7-006, REQ-DLP-024 (gRPC client for REPO-DATA-SERVICE)

            // Infrastructure Layer - Utilities
            services.AddSingleton<ModelFileLoader>(); // REQ-7-006 (uses DataServiceClient to load models)
            // services.AddSingleton<ImageProcessingUtils>(); // If SharpCV utilities are wrapped in a class

            // Shared Utilities (Logging, Monitoring)
            // Serilog is configured in Program.cs. OpenTelemetry would be added here or in Program.cs.
            // E.g., services.AddOpenTelemetryMetrics(...); services.AddOpenTelemetryTracing(...);

            return services;
        }
    }

    // Placeholder for types that would be defined in other files/projects.
    // These are here just to make DependencyInjection.cs compile standalone for this exercise.
    // In a real project, these would be in their respective locations.

    namespace Application.Interfaces { public interface IModelManagementAppService { } public interface IEdgeDeploymentAppService { } }
    namespace Application.Services { public class ModelManagementAppService : IModelManagementAppService { } public class EdgeDeploymentAppService : IEdgeDeploymentAppService { } }
    namespace Domain.Interfaces
    {
        public interface IModelRepository { }
        public interface IModelExecutionEngine { }
        public interface INlpProvider { }
        public interface IMlLopsClient { }
    }
    namespace Domain.Services
    {
        public class ModelExecutionService { public ModelExecutionService(IEnumerable<Domain.Interfaces.IModelExecutionEngine> engines, Domain.Interfaces.IModelRepository repo, Infrastructure.AI.Common.ModelFileLoader loader, ILogger<ModelExecutionService> logger) { } }
        public class NlpOrchestrationService { public NlpOrchestrationService(IEnumerable<Domain.Interfaces.INlpProvider> providers, IOptions<NlpProviderOptions> options, ILogger<NlpOrchestrationService> logger) { } }
    }
    namespace Infrastructure.AI.Common { public class ModelFileLoader { public ModelFileLoader(Clients.DataServiceClient client, IOptions<ModelOptions> options) { } } }
    namespace Infrastructure.AI.Execution.ONNX { public class OnnxExecutionEngine : Domain.Interfaces.IModelExecutionEngine { } }
    namespace Infrastructure.AI.Execution.MLNet { public class MLNetExecutionEngine : Domain.Interfaces.IModelExecutionEngine { } }
    namespace Infrastructure.AI.Execution.TensorFlow { public class TensorFlowExecutionEngine : Domain.Interfaces.IModelExecutionEngine { } }
    namespace Infrastructure.Nlp.SpaCy { public class SpaCyNlpProvider : Domain.Interfaces.INlpProvider { } }
    namespace Infrastructure.Nlp.AzureCognitive { public class AzureCognitiveServicesNlpProvider : Domain.Interfaces.INlpProvider { } }
    namespace Infrastructure.MLOps.MLflow { public class MlflowClientAdapter : Domain.Interfaces.IMlLopsClient { } }
    namespace Infrastructure.Persistence { public class ModelRepository : Domain.Interfaces.IModelRepository { public ModelRepository(Clients.DataServiceClient client) { } } }
    namespace Infrastructure.Clients { public class DataServiceClient { } }
    namespace Api.GrpcServices { public class AiProcessingGrpcService : Grpc.Core.ServerCallContext { protected override Task WriteResponseHeadersAsyncCore(Grpc.Core.Metadata responseHeaders) => Task.CompletedTask; protected override Grpc.Core.ContextPropagationToken CreatePropagationTokenCore(Grpc.Core.ContextPropagationOptions options) => default; protected override Task ProcessRequestCoreAsync() => Task.CompletedTask; protected override string MethodCore => ""; protected override string HostCore => ""; protected override string PeerCore => ""; protected override DateTime DeadlineCore => DateTime.UtcNow; protected override Grpc.Core.Metadata RequestHeadersCore => default; protected override System.Threading.CancellationToken CancellationTokenCore => default; protected override Grpc.Core.Metadata ResponseTrailersCore => default; protected override Grpc.Core.Status StatusCore { get => default; set { } } protected override Grpc.Core.WriteOptions WriteOptionsCore { get => default; set { } } protected override Grpc.Core.AuthContext AuthContextCore => default; } }
    // Temporary using directives for the placeholder types
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
}