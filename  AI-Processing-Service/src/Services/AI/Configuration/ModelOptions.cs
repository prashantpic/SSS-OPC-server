namespace AIService.Configuration
{
    /// <summary>
    /// Defines strongly-typed configuration properties for AI models,
    /// such as base paths for model storage, default model versions, etc.,
    /// to be loaded from appsettings.json.
    /// REQ-7-003, REQ-8-001
    /// </summary>
    public class ModelOptions
    {
        public const string SectionName = "ModelOptions";

        /// <summary>
        /// Base path for local model storage.
        /// This might be used if models are not solely managed via Data Service.
        /// </summary>
        public string ModelStorageBasePath { get; set; } = string.Empty;

        /// <summary>
        /// ID of the default model to be used for predictive maintenance if not specified.
        /// </summary>
        public string? DefaultPredictiveMaintenanceModelId { get; set; }

        /// <summary>
        /// Version of the default predictive maintenance model.
        /// </summary>
        public string? DefaultPredictiveMaintenanceModelVersion { get; set; }

        /// <summary>
        /// ID of the default model to be used for anomaly detection if not specified.
        /// </summary>
        public string? DefaultAnomalyDetectionModelId { get; set; }

        /// <summary>
        /// Version of the default anomaly detection model.
        /// </summary>
        public string? DefaultAnomalyDetectionModelVersion { get; set; }

        /// <summary>
        /// Flag to enable/disable in-memory caching of loaded AI models.
        /// </summary>
        public bool ModelCacheEnabled { get; set; } = true;

        /// <summary>
        /// Expiration time in minutes for cached AI models.
        /// </summary>
        public int ModelCacheExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// URI or connection string for accessing model artifacts if a central store like Azure Blob or S3 is used
        /// directly by ModelFileLoader, or handled by DataServiceClient.
        /// </summary>
        public string? ModelArtifactsStoreUri { get; set; }
    }
}