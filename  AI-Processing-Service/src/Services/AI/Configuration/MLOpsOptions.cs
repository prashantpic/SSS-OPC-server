namespace AIService.Configuration
{
    /// <summary>
    /// Defines strongly-typed configuration properties for connecting to MLOps platforms,
    /// such as service URLs, authentication tokens, and registry names.
    /// REQ-7-004, REQ-7-010
    /// </summary>
    public class MLOpsOptions
    {
        public const string SectionName = "MLOpsOptions";

        /// <summary>
        /// Specifies which IMlLopsClient implementation to use (e.g., "MLflow", "AzureML").
        /// </summary>
        public string PlatformType { get; set; } = "MLflow"; // Default to MLflow

        /// <summary>
        /// Configuration for MLflow integration.
        /// </summary>
        public MlflowOptions? Mlflow { get; set; }

        /// <summary>
        /// Configuration for Azure Machine Learning integration.
        /// </summary>
        public AzureMLOptions? AzureML { get; set; }
    }

    public class MlflowOptions
    {
        /// <summary>
        /// URI of the MLflow tracking server.
        /// Example: "http://localhost:5000" or "databricks" for Databricks-hosted MLflow.
        /// </summary>
        public string? TrackingUri { get; set; }

        /// <summary>
        /// URI of the MLflow model registry. If not set, often defaults to the TrackingUri.
        /// </summary>
        public string? RegistryUri { get; set; }

        /// <summary>
        /// Access token for authenticating with MLflow (e.g., Databricks PAT).
        /// Should be stored securely.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Default experiment name or ID to use for logging runs.
        /// </summary>
        public string? DefaultExperimentName { get; set; }
    }

    public class AzureMLOptions
    {
        /// <summary>
        /// Azure Subscription ID where the Azure ML Workspace resides.
        /// </summary>
        public string? SubscriptionId { get; set; }

        /// <summary>
        /// Name of the Azure Resource Group containing the Azure ML Workspace.
        /// </summary>
        public string? ResourceGroupName { get; set; }

        /// <summary>
        /// Name of the Azure Machine Learning Workspace.
        /// </summary>
        public string? WorkspaceName { get; set; }

        /// <summary>
        /// Tenant ID for Azure Active Directory authentication.
        /// Only needed for certain authentication flows.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Client ID for Service Principal authentication.
        /// Should be stored securely.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client Secret for Service Principal authentication.
        /// Should be stored securely.
        /// </summary>
        public string? ClientSecret { get; set; }
    }
}