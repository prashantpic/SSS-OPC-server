using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using MediatR;
using System.Reflection;
using System.Collections.Generic;
using System;

// Assuming these namespaces will exist based on the SDS and typical project structure
using AIService.Application.ModelManagement.Interfaces;
using AIService.Application.ModelManagement.Services;
using AIService.Application.EdgeDeployment.Interfaces;
using AIService.Application.EdgeDeployment.Services;
using AIService.Domain.Services;
using AIService.Domain.Interfaces;
using AIService.Infrastructure.Persistence;
using AIService.Infrastructure.AI.ONNX;
using AIService.Infrastructure.AI.TensorFlow;
using AIService.Infrastructure.AI.MLNet;
using AIService.Infrastructure.AI.Common;
using AIService.Infrastructure.Nlp.SpaCy;
using AIService.Infrastructure.Nlp.AzureCognitive;
using AIService.Infrastructure.MLOps.MLflow;
using AIService.Infrastructure.Clients;
using AIService.Infrastructure.Utils;
using AIService.Api.GrpcServices; // For AiProcessingGrpcService registration

// Assuming DataServiceClient is generated from REPO-DATA-SERVICE proto
using DataServiceProto; // Placeholder for the actual generated gRPC client namespace from REPO-DATA-SERVICE

namespace AIService.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure strongly-typed options
            services.Configure<ModelOptions>(configuration.GetSection(ModelOptions.SectionName));
            services.Configure<NlpProviderOptions>(configuration.GetSection(NlpProviderOptions.SectionName));
            services.Configure<MLOpsOptions>(configuration.GetSection(MLOpsOptions.SectionName));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register MediatR for CQRS
            // Assumes commands and handlers are in the executing assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Application Services
            services.AddScoped<IModelManagementAppService, ModelManagementAppService>();
            services.AddScoped<IEdgeDeploymentAppService, EdgeDeploymentAppService>();

            // Domain Services
            services.AddScoped<ModelExecutionService>();
            services.AddScoped<NlpOrchestrationService>();

            // Domain Interfaces & Infrastructure Implementations

            // Model Repository (using DataServiceClient)
            services.AddScoped<IModelRepository, ModelRepository>();

            // AI Execution Engines
            services.AddTransient<OnnxExecutionEngine>();
            services.AddTransient<TensorFlowExecutionEngine>();
            services.AddTransient<MLNetExecutionEngine>();
            services.AddTransient<IEnumerable<IModelExecutionEngine>>(sp =>
            {
                var engines = new List<IModelExecutionEngine>
                {
                    sp.GetRequiredService<OnnxExecutionEngine>(),
                    sp.GetRequiredService<TensorFlowExecutionEngine>(),
                    sp.GetRequiredService<MLNetExecutionEngine>()
                };
                return engines;
            });


            // NLP Providers
            services.AddTransient<SpaCyNlpProvider>();
            services.AddTransient<AzureCognitiveServicesNlpProvider>();
            services.AddTransient<IEnumerable<INlpProvider>>(sp =>
            {
                var providers = new List<INlpProvider>();
                // Conditionally add providers based on configuration
                var nlpOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NlpProviderOptions>>().Value;
                if (nlpOptions?.ActiveProvider?.Equals("SpaCy", StringComparison.OrdinalIgnoreCase) == true)
                {
                    providers.Add(sp.GetRequiredService<SpaCyNlpProvider>());
                }
                if (nlpOptions?.ActiveProvider?.Equals("Azure", StringComparison.OrdinalIgnoreCase) == true) // Or a more specific name
                {
                     providers.Add(sp.GetRequiredService<AzureCognitiveServicesNlpProvider>());
                }
                // Default or fallback
                if (providers.Count == 0 && sp.GetService<SpaCyNlpProvider>() != null) // Example: default to SpaCy if configured
                {
                    providers.Add(sp.GetRequiredService<SpaCyNlpProvider>());
                }
                return providers;
            });


            // MLOps Client (Example: MLflow, can be made configurable)
            services.AddScoped<IMlLopsClient, MlflowClientAdapter>(); // Or use a factory for multiple MLOps platforms

            // Infrastructure Utilities
            services.AddScoped<ModelFileLoader>();
            services.AddSingleton<ImageProcessingUtils>(); // Assuming stateless

            // Infrastructure Clients
            // REQ-7-006: Central AI Model Storage (via Data Service)
            // REQ-DLP-024: Store AI Model Artifacts (via Data Service)
            var dataServiceUrl = configuration["DataServiceGrpcUrl"];
            if (string.IsNullOrWhiteSpace(dataServiceUrl))
            {
                // Log warning or throw if DataService URL is critical and not configured
                // For now, let it be, startup might fail later if service is used.
                // throw new ArgumentNullException("DataServiceGrpcUrl", "DataService gRPC URL is not configured.");
            }
            services.AddGrpcClient<DataServiceClient>(o => // This is the client FROM REPO-DATA-SERVICE
            {
                 if (!string.IsNullOrWhiteSpace(dataServiceUrl))
                 {
                    o.Address = new Uri(dataServiceUrl);
                 }
                 // Add other client configurations like interceptors, credentials if needed
            });
            // If DataServiceClient is a wrapper class (as per SDS section 5.4), register it here:
            // services.AddScoped<AIService.Infrastructure.Clients.DataServiceClient>(); // Wrapper class
            // The above line refers to our *own* wrapper, not the gRPC generated one.
            // For now, assuming direct use of generated client or the wrapper is also named DataServiceClient.
            // Let's assume the DataServiceClient in AIService.Infrastructure.Clients is the wrapper.
            services.AddScoped<AIService.Infrastructure.Clients.DataServiceClient>();


            // Register gRPC Services for this AI service itself
            services.AddScoped<AiProcessingGrpcService>(); // So it can be mapped in Program.cs

            return services;
        }
    }
}