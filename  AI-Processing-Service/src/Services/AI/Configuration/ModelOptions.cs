namespace AIService.Configuration
{
    public class ModelOptions
    {
        public const string SectionName = "ModelOptions";

        public string ModelStorageBasePath { get; set; } = string.Empty;
        public string DefaultPredictiveMaintenanceModelId { get; set; } = string.Empty;
        public string DefaultAnomalyDetectionModelId { get; set; } = string.Empty;
        public bool ModelCacheEnabled { get; set; } = true;
        public int ModelCacheExpirationMinutes { get; set; } = 60;
    }
}