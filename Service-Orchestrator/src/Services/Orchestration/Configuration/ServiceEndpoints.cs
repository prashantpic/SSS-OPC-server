namespace OrchestrationService.Configuration;

/// <summary>
/// Stores the base URLs or connection details for dependent microservices.
/// These settings are loaded from `appsettings.json` and used by HTTP client implementations
/// to communicate with external services.
/// </summary>
public class ServiceEndpoints
{
    /// <summary>
    /// Gets or sets the base URL for the Management Service.
    /// This service is used for configuration, user/role information (RBAC), etc.
    /// </summary>
    public string ManagementServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the AI Processing Service.
    /// This service is responsible for AI analysis tasks.
    /// </summary>
    public string AiServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Data Service.
    /// This service handles historical data retrieval, report archiving, and off-chain storage.
    /// </summary>
    public string DataServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Integration Service.
    /// This service manages report distribution, blockchain commitment, and other external system interactions.
    /// </summary>
    public string IntegrationServiceUrl { get; set; } = string.Empty;
}