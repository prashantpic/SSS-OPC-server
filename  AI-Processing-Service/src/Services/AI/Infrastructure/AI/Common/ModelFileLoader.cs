```csharp
using AIService.Infrastructure.Clients; // Assuming DataServiceClient is here
using AIService.Infrastructure.Configuration; // For ModelOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AIService.Infrastructure.AI.Common
{
    public class ModelFileLoader
    {
        private readonly ILogger<ModelFileLoader> _logger;
        private readonly DataServiceClient _dataServiceClient; // Optional, if models can be fetched from DataService
        private readonly ModelOptions _modelOptions;

        public ModelFileLoader(
            ILogger<ModelFileLoader> logger,
            IOptions<ModelOptions> modelOptions,
            DataServiceClient dataServiceClient = null) // DataServiceClient is optional
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelOptions = modelOptions?.Value ?? throw new ArgumentNullException(nameof(modelOptions));
            _dataServiceClient = dataServiceClient; // Can be null if not configured/used
        }

        /// <summary>
        /// Loads a model file into a stream.
        /// </summary>
        /// <param name="storageReference">
        /// A reference to the model's storage location.
        /// This could be a local file path, an identifier for DataService (e.g., "dataservice://model-id/version"),
        /// or a URI.
        /// </param>
        /// <param name="modelNameForLog">Optional model name for logging.</param>
        /// <returns>A stream containing the model data. The caller is responsible for disposing the stream.</returns>
        /// <exception cref="FileNotFoundException">If the model file cannot be found.</exception>
        /// <exception cref="ArgumentException">If the storage reference is invalid.</exception>
        public async Task<Stream> LoadModelFileAsync(string storageReference, string modelNameForLog = null)
        {
            if (string.IsNullOrWhiteSpace(storageReference))
            {
                throw new ArgumentException("Storage reference cannot be null or whitespace.", nameof(storageReference));
            }

            modelNameForLog = modelNameForLog ?? storageReference;
            _logger.LogInformation("Attempting to load model file '{ModelName}' from storage reference: {StorageReference}", modelNameForLog, storageReference);

            // Strategy 1: Check for DataService specific scheme
            if (storageReference.StartsWith("dataservice://", StringComparison.OrdinalIgnoreCase))
            {
                if (_dataServiceClient == null)
                {
                    _logger.LogError("DataServiceClient is not configured, cannot load model '{ModelName}' from DataService reference: {StorageReference}", modelNameForLog, storageReference);
                    throw new InvalidOperationException("DataServiceClient is not available to handle 'dataservice://' references.");
                }
                
                // Parse modelId and version from storageReference (e.g., "dataservice://<modelId>/<version>")
                var parts = storageReference.Substring("dataservice://".Length).Split('/');
                if (parts.Length < 1) // At least modelId is needed. Version might be optional or handled by DataServiceClient.
                {
                     _logger.LogError("Invalid DataService storage reference format for model '{ModelName}': {StorageReference}. Expected 'dataservice://modelId[/version]'.", modelNameForLog, storageReference);
                    throw new ArgumentException("Invalid DataService storage reference format. Expected 'dataservice://modelId[/version]'.", nameof(storageReference));
                }
                string modelId = parts[0];
                string version = parts.Length > 1 ? parts[1] : null; // Version is optional

                _logger.LogDebug("Fetching model '{ModelName}' (ID: {ModelId}, Version: {Version}) from DataService.", modelNameForLog, modelId, version ?? "latest");
                try
                {
                    var modelArtifactStream = await _dataServiceClient.GetModelArtifactStreamAsync(modelId, version);
                    if (modelArtifactStream == null || modelArtifactStream == Stream.Null || (modelArtifactStream.CanSeek && modelArtifactStream.Length == 0) )
                    {
                        _logger.LogError("Model artifact for '{ModelName}' (ID: {ModelId}, Version: {Version}) not found or is empty in DataService.", modelNameForLog, modelId, version ?? "latest");
                        throw new FileNotFoundException($"Model artifact for '{modelNameForLog}' (ID: {modelId}, Version: {version ?? "latest"}) not found or is empty in DataService.");
                    }
                    _logger.LogInformation("Model file '{ModelName}' loaded successfully from DataService.", modelNameForLog);
                    return modelArtifactStream;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load model '{ModelName}' (ID: {ModelId}, Version: {Version}) from DataService.", modelNameForLog, modelId, version ?? "latest");
                    throw; // Re-throw to allow execution engine to handle
                }
            }

            // Strategy 2: Check for absolute path (local file system)
            // This might be restricted for security in a service environment.
            // Use ModelOptions.ModelStorageBasePath as a prefix if storageReference is relative.
            string fullPath = storageReference;
            if (!Path.IsPathRooted(storageReference))
            {
                if (string.IsNullOrWhiteSpace(_modelOptions.ModelStorageBasePath))
                {
                    _logger.LogError("ModelStorageBasePath is not configured. Cannot resolve relative path for model '{ModelName}': {StorageReference}", modelNameForLog, storageReference);
                    throw new InvalidOperationException("ModelStorageBasePath is not configured for relative model paths.");
                }
                fullPath = Path.Combine(_modelOptions.ModelStorageBasePath, storageReference);
            }

            if (File.Exists(fullPath))
            {
                _logger.LogInformation("Loading model file '{ModelName}' from local path: {FullPath}", modelNameForLog, fullPath);
                try
                {
                    // Return a new FileStream. The caller is responsible for disposing it.
                    // Open with read-only and allow shared read access.
                    return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Failed to open model file '{ModelName}' from local path: {FullPath}", modelNameForLog, fullPath);
                    throw; // Re-throw
                }
            }
            
            // Strategy 3: Interpret as URI (e.g., HTTP/HTTPS) - Not implemented here for brevity, would need HttpClient
            if (Uri.TryCreate(storageReference, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                _logger.LogWarning("Loading model from HTTP/HTTPS URI ({Uri}) is not implemented in this version of ModelFileLoader.", storageReference);
                throw new NotImplementedException("Loading models from HTTP/HTTPS URIs is not yet implemented.");
            }

            _logger.LogError("Model file '{ModelName}' not found at storage reference: {StorageReference} (resolved to: {ResolvedPath}) and not a recognized scheme.", modelNameForLog, storageReference, fullPath);
            throw new FileNotFoundException($"Model file '{modelNameForLog}' not found or accessible via reference: {storageReference}.", storageReference);
        }
    }
}