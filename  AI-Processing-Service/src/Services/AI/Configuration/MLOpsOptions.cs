namespace AIService.Configuration
{
    public class MLOpsOptions
    {
        public const string SectionName = "MLOpsOptions";

        public string? PlatformType { get; set; } // e.g., "MLflow", "AzureML"
        public string? MlflowTrackingUri { get; set; }

        // Azure ML specific settings
        public string? AzureMLWorkspaceName { get; set; }
        public string? AzureMLSubscriptionId { get; set; }
        public string? AzureMLResourceGroup { get; set; }
        public string? AzureMLRegistryName { get; set; } // If using Azure ML Registries
    }
}