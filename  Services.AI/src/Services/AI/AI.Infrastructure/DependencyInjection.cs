using Azure.AI.Language.QuestionAnswering;
using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;
using Opc.System.Services.AI.Domain.Interfaces;
using Opc.System.Services.AI.Infrastructure.Nlp;
using Opc.System.Services.AI.Infrastructure.Persistence;
using Opc.System.Services.AI.Infrastructure.Services;
using Opc.System.Services.AI.Infrastructure.Stores;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Encapsulates the registration of all Infrastructure layer dependencies, such as
/// repository implementations, external service clients, and the ONNX model runner.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Model Storage (Azure Blob Storage)
        services.Configure<AzureStorageConfig>(configuration.GetSection("Azure:Storage"));
        services.AddSingleton(x => new BlobServiceClient(configuration["Azure:Storage:ConnectionString"]));
        services.AddScoped<IModelStore, BlobModelStore>();

        // Register Model Runner
        services.AddSingleton<IModelRunner, OnnxModelRunner>();

        // Register NLP Service (Azure Cognitive Service for Language)
        var cognitiveServicesSection = configuration.GetSection("Azure:CognitiveServices");
        services.AddSingleton(new QuestionAnsweringClient(
            new Uri(cognitiveServicesSection["Endpoint"]!),
            new AzureKeyCredential(cognitiveServicesSection["ApiKey"]!)));
        
        services.AddScoped<INlpServiceProvider, AzureNlpServiceProvider>();
        services.AddSingleton<INlpServiceFactory>(provider =>
        {
            // This factory could be extended to support feature toggles (e.g., UseGoogleNlp)
            return new NlpServiceFactory(provider);
        });

        // Register Data Service Client (gRPC)
        // In a real application, the proto-generated client would be registered.
        // Here, we register a dummy implementation for demonstration.
        services.AddScoped<IDataServiceClient, DummyDataServiceClient>();
        // Example of real gRPC client registration:
        // services.AddGrpcClient<DataService.DataServiceClient>(o =>
        // {
        //     o.Address = new Uri(configuration.GetConnectionString("DataServiceGrpc")!);
        // });

        // Register Repositories
        // These would typically be implemented using a database context (e.g., EF Core)
        // that communicates with the Data Service or a dedicated database.
        // Here, we register dummy implementations.
        services.AddScoped<IAiModelRepository, DummyAiModelRepository>();
        services.AddScoped<INlqAliasRepository, DummyNlqAliasRepository>();
        
        return services;
    }
}

// Dummy/Placeholder implementations for services whose concrete implementation is out of scope.
namespace Opc.System.Services.AI.Infrastructure.Persistence
{
    using Opc.System.Services.AI.Domain.Aggregates;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class DummyAiModelRepository : IAiModelRepository
    {
        private static readonly Dictionary<Guid, AiModel> _models = new();
        public Task AddAsync(AiModel model, CancellationToken cancellationToken = default) { _models[model.Id] = model; return Task.CompletedTask; }
        public Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_models.GetValueOrDefault(id));
        public Task UpdateAsync(AiModel model, CancellationToken cancellationToken = default) { _models[model.Id] = model; return Task.CompletedTask; }
    }

    internal class DummyNlqAliasRepository : INlqAliasRepository
    {
        private static readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase) { { "Boiler 1 Temperature", "ns=2;s=Boiler1.Temp" } };
        public Task<string?> ResolveAliasAsync(string alias, CancellationToken cancellationToken) => Task.FromResult(_aliases.GetValueOrDefault(alias));
    }
}

namespace Opc.System.Services.AI.Infrastructure.Services
{
    internal class DummyDataServiceClient : IDataServiceClient
    {
        public Task<IEnumerable<object>> GetHistoricalDataAsync(string tagId, DateTime start, DateTime end, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Range(0, 10).Select(i => new { Timestamp = start.AddHours(i), Value = 100.0 + i } as object));
        public Task<IEnumerable<object>> GetHistoricalDataForAssetAsync(Guid assetId, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Range(0, 100).Select(i => new { Timestamp = DateTime.UtcNow.AddMinutes(-100+i), Value = 50.0 + Math.Sin(i/10.0) } as object));
        public Task<object?> GetLatestValueAsync(string tagId, CancellationToken cancellationToken) => Task.FromResult((object?)new { Timestamp = DateTime.UtcNow, Value = 123.45 });
    }
}