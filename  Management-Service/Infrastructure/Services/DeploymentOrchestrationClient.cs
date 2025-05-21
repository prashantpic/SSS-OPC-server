using System.Net.Http.Json; // For PostAsJsonAsync

namespace ManagementService.Infrastructure.Services;

// Interface for the deployment orchestration client
public interface IDeploymentOrchestrationClient
{
    Task ScheduleBulkUpdateAsync(
        Guid jobId,
        List<Guid> clientIds,
        string updatePackageUrl,
        string targetVersion,
        CancellationToken cancellationToken
    );
}


public class DeploymentOrchestrationClient : IDeploymentOrchestrationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeploymentOrchestrationClient> _logger;

    public DeploymentOrchestrationClient(IHttpClientFactory httpClientFactory, ILogger<DeploymentOrchestrationClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DeploymentOrchestrationClient"); // Named client from Program.cs
        _logger = logger;
    }

    public async Task ScheduleBulkUpdateAsync(
        Guid jobId,
        List<Guid> clientIds,
        string updatePackageUrl,
        string targetVersion,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating software update via DeploymentOrchestrationService for JobId: {JobId}", jobId);

        var requestPayload = new DeploymentRequestPayload
        {
            ManagementJobId = jobId,
            ClientInstanceIds = clientIds,
            PackageUrl = updatePackageUrl,
            TargetSoftwareVersion = targetVersion
        };

        try
        {
            // Assuming endpoint: POST /api/deployments/schedule-update
            var response = await _httpClient.PostAsJsonAsync("api/deployments/schedule-update", requestPayload, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully scheduled software update with DeploymentOrchestrationService for JobId: {JobId}", jobId);
                // Optionally process response if it contains an external job ID or further details
                // var externalJobInfo = await response.Content.ReadFromJsonAsync<ExternalJobInfo>(cancellationToken);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to schedule software update with DeploymentOrchestrationService for JobId: {JobId}. Status: {StatusCode}, Response: {ErrorContent}", 
                    jobId, response.StatusCode, errorContent);
                // Throw an exception to indicate failure to the calling handler
                response.EnsureSuccessStatusCode(); 
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with DeploymentOrchestrationService for JobId: {JobId}", jobId);
            throw; // Allow Polly policies and error middleware to handle
        }
    }
}

// Internal DTO for request payload to the external service
internal class DeploymentRequestPayload
{
    public Guid ManagementJobId { get; set; }
    public List<Guid> ClientInstanceIds { get; set; } = new List<Guid>();
    public string PackageUrl { get; set; } = string.Empty;
    public string TargetSoftwareVersion { get; set; } = string.Empty;
    // Add any other fields required by the external deployment orchestration service
}