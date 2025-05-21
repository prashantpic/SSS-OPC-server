using AIService.Infrastructure.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AIService.Infrastructure.AI.Common
{
    public class ModelFileLoader
    {
        private readonly ILogger<ModelFileLoader> _logger;
        private readonly DataServiceClient _dataServiceClient; // Assuming this client can fetch model artifacts
        private readonly IConfiguration _configuration;
        private readonly string _localModelBasePath;

        public ModelFileLoader(
            ILogger<ModelFileLoader> logger,
            DataServiceClient dataServiceClient, // Can be null if DataService is not used for artifacts
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataServiceClient = dataServiceClient; // Allowed to be null
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _localModelBasePath = _configuration["ModelOptions:ModelStorageBasePath"]; // Example config key
        }

        /// <summary>
        /// Loads a model file stream based on its storage reference.
        /// The storageReference can be a local file path or an identifier for the Data Service.
        /// </summary>
        /// <param name="storageReference">Identifier for the model file.
        /// Examples:
        /// - "dataservice://models/pm_model_v1.onnx" (protocol indicates source)
        /// - "model_artifact_id_12345" (an ID to be resolved by DataServiceClient)
        /// - "/mnt/models/local_model.pb" (an absolute local path)
        /// - "relative/path/to/model.zip" (a relative local path, resolved against _localModelBasePath)
        /// </param>
        /// <returns>A Stream containing the model file, or null if not found.</returns>
        public async Task<Stream> LoadModelFileAsync(string storageReference)
        {
            if (string.IsNullOrWhiteSpace(storageReference))
            {
                _logger.LogError("Storage reference for model file is null or empty.");
                throw new ArgumentNullException(nameof(storageReference));
            }

            _logger.LogInformation("Attempting to load model file from storage reference: {StorageReference}", storageReference);

            try
            {
                // Scheme 1: URI-based reference (e.g., "dataservice://<id>", "file://<path>")
                if (Uri.TryCreate(storageReference, UriKind.Absolute, out Uri uri))
                {
                    if (uri.Scheme.Equals("dataservice", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_dataServiceClient == null)
                        {
                            _logger.LogError("DataServiceClient is not configured, cannot load model from dataservice URI: {Uri}", uri);
                            throw new InvalidOperationException("DataServiceClient is not available to fetch model artifacts from Data Service.");
                        }
                        var modelId = uri.Host + uri.PathAndQuery; // Or parse path segments more carefully
                         _logger.LogInformation("Fetching model artifact '{ModelId}' from Data Service.", modelId);
                        // Assuming DataServiceClient has a method like GetModelArtifactStreamAsync(string modelId)
                        return await _dataServiceClient.GetModelArtifactStreamAsync(modelId);
                    }
                    else if (uri.IsFile)
                    {
                        _logger.LogInformation("Loading model from local file URI: {FilePath}", uri.LocalPath);
                        if (File.Exists(uri.LocalPath))
                        {
                            return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                        }
                        else
                        {
                            _logger.LogError("Model file not found at local URI path: {FilePath}", uri.LocalPath);
                            return null;
                        }
                    }
                    // Add other schemes like "http://", "https://" if models can be fetched from URLs
                }

                // Scheme 2: Raw ID for DataService (heuristic: not a path, not a full URI)
                // This is a weaker heuristic. A prefix like "ds:" might be better.
                bool looksLikeId = !storageReference.Contains(Path.DirectorySeparatorChar) && !storageReference.Contains(Path.AltDirectorySeparatorChar) && !storageReference.Contains(":");
                if (looksLikeId && _dataServiceClient != null)
                {
                    _logger.LogInformation("Assuming '{StorageReference}' is a Data Service artifact ID. Fetching...", storageReference);
                     // This is a fallback, prefer explicit "dataservice://" scheme
                    var stream = await _dataServiceClient.GetModelArtifactStreamAsync(storageReference);
                    if (stream != null) return stream;
                    _logger.LogWarning("Model artifact ID '{StorageReference}' not found via Data Service. Trying local path next.", storageReference);
                }


                // Scheme 3: Local file path (absolute or relative to base path)
                string localPath = storageReference;
                if (!Path.IsPathRooted(localPath) && !string.IsNullOrEmpty(_localModelBasePath))
                {
                    localPath = Path.Combine(_localModelBasePath, localPath);
                    _logger.LogInformation("Resolved relative model path to: {FullPath}", localPath);
                }
                else if (!Path.IsPathRooted(localPath))
                {
                    // Relative path without a base path - try relative to application base
                    localPath = Path.Combine(AppContext.BaseDirectory, localPath);
                     _logger.LogInformation("Resolved relative model path (no base path configured) to: {FullPath}", localPath);
                }


                if (File.Exists(localPath))
                {
                    _logger.LogInformation("Loading model from local file system path: {FilePath}", localPath);
                    return new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                }
                else
                {
                    _logger.LogError("Model file not found at resolved local path: {FilePath}", localPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading model file from storage reference: {StorageReference}", storageReference);
                throw; // Or return null depending on desired error handling
            }

            _logger.LogWarning("Failed to load model from storage reference: {StorageReference} using any known method.", storageReference);
            return null;
        }

        // Consider adding caching mechanisms here if models are frequently reloaded
        // and DataServiceClient/FileStream access is expensive.
        // For example, an in-memory cache for model byte arrays with an eviction policy.
    }
}