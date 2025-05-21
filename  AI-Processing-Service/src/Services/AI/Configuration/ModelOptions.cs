namespace AIService.Configuration
{
    public class ModelOptions
    {
        public const string SectionName = "ModelOptions";

        public string? ModelStorageBasePath { get; set; }
        public string? DefaultPredictiveMaintenanceModelId { get; set; }
        public string? DefaultAnomalyDetectionModelId { get; set; }
        public bool ModelCacheEnabled { get; set; } = true;
        public int ModelCacheExpirationMinutes { get; set; } = 60;
    }
}