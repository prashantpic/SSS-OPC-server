using AIService.Domain.Models; // For AiModel
using AIService.Configuration; // For DataServiceOptions
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Assuming REPO-DATA-SERVICE exposes a gRPC service like 'DataStoreService'
// defined in a .proto file. We need to placeholder the generated client types.
// Example proto:
// service DataStoreService {
//   rpc GetAiModelMetadata (GetAiModelMetadataRequest) returns (AiModelMetadataResponse);
//   rpc SaveAiModelMetadata (AiModel) returns (google.protobuf.Empty); // Assuming AiModel is a message type
//   rpc GetAiModelArtifact (GetAiModelArtifactRequest) returns (stream AiModelArtifactChunk);
//   rpc StoreAiModelArtifact (stream StoreAiModelArtifactRequest) returns (google.protobuf.Empty);
//   rpc ListAiModels (ListAiModelsRequest) returns (ListAiModelsResponse);
// }
// For this example, I'll use placeholder types like DataStore.DataStoreClient,
// DataStore.GetAiModelMetadataRequest, DataStore.AiModel (as a message), etc.
// These would be generated from the actual .proto file of REPO-DATA-SERVICE.

// BEGIN Placeholder for gRPC generated types (normally in a separate file from .proto)
namespace DataStore
{
    // These are illustrative. Actual types depend on REPO-DATA-SERVICE's .proto definition.
    public class DataStoreClient // Simulates the generated gRPC client
    {
        private readonly CallInvoker _callInvoker;
        public DataStoreClient(CallInvoker callInvoker) { _callInvoker = callInvoker; }
        public virtual AsyncUnaryCall<AiModelMetadataResponse> GetAiModelMetadataAsync(GetAiModelMetadataRequest request, CallOptions options) => throw new NotImplementedException();
        public virtual AsyncUnaryCall<Google.Protobuf.WellKnownTypes.Empty> SaveAiModelMetadataAsync(AIService.Domain.Models.AiModel request, CallOptions options) => throw new NotImplementedException(); // Pass Domain model directly if proto matches
        public virtual AsyncServerStreamingCall<AiModelArtifactChunk> GetAiModelArtifact(GetAiModelArtifactRequest request, CallOptions options) => throw new NotImplementedException();
        public virtual AsyncClientStreamingCall<StoreAiModelArtifactRequest, Google.Protobuf.WellKnownTypes.Empty> StoreAiModelArtifact(CallOptions options) => throw new NotImplementedException();
        public virtual AsyncUnaryCall<ListAiModelsResponse> ListAiModelsAsync(ListAiModelsRequest request, CallOptions options) => throw new NotImplementedException();

    }

    public class GetAiModelMetadataRequest { public string ModelId { get; set; } public string Version { get; set; } }
    public class AiModelMetadataResponse { public AIService.Domain.Models.AiModel Model { get; set; } } // Assuming it directly returns the domain model
    public class GetAiModelArtifactRequest { public string ModelId { get; set; } public string Version { get; set; } }
    public class AiModelArtifactChunk { public Google.Protobuf.ByteString Content { get; set; } }
    public class StoreAiModelArtifactRequest {
        public FileMetadata Metadata { get; set; }
        public Google.Protobuf.ByteString Content { get; set; }
        public class FileMetadata { public string ModelId { get; set; } public string Version { get; set; } public string FileName { get; set; } }
    }
    public class ListAiModelsRequest { public string ModelTypeFilter {get;set;} public string ModelFormatFilter {get;set;}}
    public class ListAiModelsResponse { public List<AIService.Domain.Models.AiModel> Models {get;set;} }

}
namespace Google.Protobuf.WellKnownTypes { public class Empty { } }
// END Placeholder

namespace AIService.Infrastructure.Clients
{
    public class DataServiceClient
    {
        private readonly DataStore.DataStoreClient _client; // Placeholder for actual generated gRPC client
        private readonly ILogger<DataServiceClient> _logger;
        private readonly DataServiceOptions _options;

        public DataServiceClient(ILogger<DataServiceClient> logger, IOptions<DataServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.GrpcUrl))
            {
                _logger.LogError("DataService gRPC URL is not configured.");
                throw new InvalidOperationException("DataService gRPC URL is not configured.");
            }

            var channel = GrpcChannel.ForAddress(_options.GrpcUrl);
            // Replace DataStore.DataStoreClient with the actual generated gRPC client type
            _client = new DataStore.DataStoreClient(channel);
            _logger.LogInformation("DataServiceClient initialized for gRPC URL: {DataServiceUrl}", _options.GrpcUrl);
        }

        public async Task<AiModel> GetAiModelMetadataAsync(string modelId, string version = null)
        {
            _logger.LogDebug("Requesting AiModel metadata from DataService for ModelId: {ModelId}, Version: {Version}", modelId, version ?? "latest");
            try
            {
                var request = new DataStore.GetAiModelMetadataRequest { ModelId = modelId, Version = version ?? string.Empty };
                var response = await _client.GetAiModelMetadataAsync(request, new CallOptions(deadline: DateTime.UtcNow.AddSeconds(_options.DefaultTimeoutSeconds)));
                return response?.Model; // Assuming the response directly contains the AiModel domain object or needs mapping
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while getting AiModel metadata for ModelId: {ModelId}. Status: {StatusCode}", modelId, rpcEx.StatusCode);
                if (rpcEx.StatusCode == StatusCode.NotFound) return null;
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AiModel metadata for ModelId: {ModelId}", modelId);
                throw;
            }
        }

        public async Task SaveAiModelMetadataAsync(AiModel model)
        {
            _logger.LogDebug("Saving AiModel metadata to DataService for ModelId: {ModelId}", model.Id);
            try
            {
                // Assuming the gRPC service directly accepts the AiModel domain object if proto is designed that way.
                // Otherwise, map AiModel to the gRPC request message type.
                await _client.SaveAiModelMetadataAsync(model, new CallOptions(deadline: DateTime.UtcNow.AddSeconds(_options.DefaultTimeoutSeconds)));
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while saving AiModel metadata for ModelId: {ModelId}. Status: {StatusCode}", model.Id, rpcEx.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AiModel metadata for ModelId: {ModelId}", model.Id);
                throw;
            }
        }

        public async Task<Stream> GetModelArtifactStreamAsync(string modelId, string version = null)
        {
            _logger.LogDebug("Requesting model artifact stream from DataService for ModelId: {ModelId}, Version: {Version}", modelId, version ?? "latest");
            try
            {
                var request = new DataStore.GetAiModelArtifactRequest { ModelId = modelId, Version = version ?? string.Empty };
                var call = _client.GetAiModelArtifact(request, new CallOptions(deadline: DateTime.UtcNow.AddSeconds(_options.StreamingTimeoutSeconds))); // Longer timeout for streams

                var memoryStream = new MemoryStream(); // Accumulate chunks here
                await foreach (var chunk in call.ResponseStream.ReadAllAsync())
                {
                    if (chunk.Content != null)
                    {
                        chunk.Content.WriteTo(memoryStream);
                    }
                }
                memoryStream.Position = 0;
                if (memoryStream.Length == 0)
                {
                    _logger.LogWarning("Received empty artifact stream for ModelId: {ModelId}", modelId);
                    await memoryStream.DisposeAsync();
                    return null;
                }
                return memoryStream;
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while getting model artifact stream for ModelId: {ModelId}. Status: {StatusCode}", modelId, rpcEx.StatusCode);
                 if (rpcEx.StatusCode == StatusCode.NotFound) return null;
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model artifact stream for ModelId: {ModelId}", modelId);
                throw;
            }
        }
         public async Task StoreModelArtifactAsync(string modelId, string version, Stream artifactStream, string fileName)
        {
            _logger.LogDebug("Storing model artifact to DataService for ModelId: {ModelId}, Version: {Version}, FileName: {FileName}", modelId, version, fileName);
            try
            {
                using var call = _client.StoreAiModelArtifact(new CallOptions(deadline: DateTime.UtcNow.AddSeconds(_options.StreamingTimeoutSeconds)));
                
                // Send metadata first (if proto defines it this way)
                var metadataRequest = new DataStore.StoreAiModelArtifactRequest
                {
                    Metadata = new DataStore.StoreAiModelArtifactRequest.FileMetadata { ModelId = modelId, Version = version, FileName = fileName }
                };
                // If metadata is part of the first message with content, adjust logic.
                // This example assumes a separate metadata message or that it's part of each chunk's wrapper.
                // A common pattern is: first message has metadata, subsequent have chunks.
                // Or, metadata is sent, then stream of chunks. Let's assume the latter simplified.
                
                // Send first message with metadata (if required by proto design this way)
                // await call.RequestStream.WriteAsync(new DataStore.StoreAiModelArtifactRequest { Metadata = metadataPacket });


                byte[] buffer = new byte[64 * 1024]; // 64KB chunks
                int bytesRead;
                bool firstChunk = true;

                while ((bytesRead = await artifactStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunkRequest = new DataStore.StoreAiModelArtifactRequest
                    {
                        Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
                    };
                    if (firstChunk) // Send metadata with the first chunk if proto combines them
                    {
                        chunkRequest.Metadata = new DataStore.StoreAiModelArtifactRequest.FileMetadata { ModelId = modelId, Version = version, FileName = fileName };
                        firstChunk = false;
                    }
                    await call.RequestStream.WriteAsync(chunkRequest);
                }
                await call.RequestStream.CompleteAsync();
                await call.ResponseAsync; // Wait for server to acknowledge completion

                _logger.LogInformation("Successfully streamed model artifact for ModelId: {ModelId}", modelId);
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while storing model artifact for ModelId: {ModelId}. Status: {StatusCode}", modelId, rpcEx.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing model artifact for ModelId: {ModelId}", modelId);
                throw;
            }
        }

        public async Task<IEnumerable<AiModel>> ListAiModelsAsync(string modelTypeFilter = null, string modelFormatFilter = null)
        {
             _logger.LogDebug("Requesting list of AiModels from DataService. TypeFilter: {TypeFilter}, FormatFilter: {FormatFilter}", modelTypeFilter, modelFormatFilter);
            try
            {
                var request = new DataStore.ListAiModelsRequest {
                    ModelTypeFilter = modelTypeFilter ?? string.Empty,
                    ModelFormatFilter = modelFormatFilter ?? string.Empty
                };
                var response = await _client.ListAiModelsAsync(request, new CallOptions(deadline: DateTime.UtcNow.AddSeconds(_options.DefaultTimeoutSeconds)));
                return response?.Models ?? new List<AiModel>();
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error while listing AiModels. Status: {StatusCode}", rpcEx.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing AiModels.");
                throw;
            }
        }
    }
}