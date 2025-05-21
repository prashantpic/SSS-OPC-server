namespace AIService.Configuration
{
    public class MLOpsOptions
    {
        public const string SectionName = "MLOpsOptions";

        public string PlatformType { get; set; } = "MLflow"; // e.g., "MLflow", "AzureML"
        public string? MlflowTrackingUri { get; set; }
        public string? AzureMLWorkspaceName { get; set; }
        public string? AzureMLSubscriptionId { get; set; }
        public string? AzureMLResourceGroup { get; set; }
        public string? ApiKey { get; set; } // Generic API key or token for MLOps platform
    }
}