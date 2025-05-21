namespace OrchestrationService.Configuration;

/// <summary>
/// Stores the base URLs or connection details for dependent microservices.
/// These settings are loaded from the "ServiceEndpoints" section in `appsettings.json`.
/// Used by HTTP client implementations.
/// </summary>
public class ServiceEndpoints
{
    /// <summary>
    /// Gets or sets the base URL for the AI Processing Service.
    /// </summary>
    public string? AiServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Data Service.
    /// </summary>
    public string? DataServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Integration Service.
    /// </summary>
    public string? IntegrationServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Management Service.
    /// </summary>
    public string? ManagementServiceUrl { get; set; }
}