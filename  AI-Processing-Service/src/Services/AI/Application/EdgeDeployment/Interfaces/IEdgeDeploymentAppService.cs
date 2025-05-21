using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIService.Application.EdgeDeployment.Models.Results; // Placeholder for result models

namespace AIService.Application.EdgeDeployment.Interfaces
{
    /// <summary>
    /// Defines contracts for orchestrating the deployment of AI models to edge devices.
    /// REQ-8-001: Edge AI Model Deployment.
    /// </summary>
    public interface IEdgeDeploymentAppService
    {
        /// <summary>
        /// Initiates the deployment of a specified AI model to a list of target edge devices.
        /// </summary>
        /// <param name="modelId">The ID of the model to deploy.</param>
        /// <param name="modelVersion">The specific version of the model to deploy. If null, latest or recommended might be used.</param>
        /// <param name="deviceIds">A collection of unique identifiers for the target edge devices.</param>
        /// <param name="deploymentConfiguration">Optional configuration parameters for the deployment (e.g., priority, resource limits on edge).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result object indicating the initiation of the deployment process.</returns>
        Task<EdgeDeploymentOperationResult> DeployModelToEdgeAsync(
            string modelId,
            string? modelVersion,
            IEnumerable<string> deviceIds,
            Dictionary<string, string>? deploymentConfiguration,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the status of an ongoing or completed edge deployment.
        /// </summary>
        /// <param name="deploymentId">The unique identifier of the deployment task.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the deployment status across targeted devices.</returns>
        Task<EdgeDeploymentStatusInfo> GetEdgeDeploymentStatusAsync(
            string deploymentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists models currently deployed or suitable for deployment on a specific edge device.
        /// </summary>
        /// <param name="deviceId">The ID of the edge device.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of models relevant to the specified edge device.</returns>
        Task<IEnumerable<EdgeModelInfo>> GetModelsForDeviceAsync(
            string deviceId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Recalls/Undeploys a model from specified edge devices.
        /// </summary>
        /// <param name="modelId">The ID of the model to recall.</param>
        /// <param name="modelVersion">The specific version of the model to recall.</param>
        /// <param name="deviceIds">A collection of device IDs from which to recall the model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result object indicating the initiation of the recall process.</returns>
        Task<EdgeDeploymentOperationResult> RecallModelFromEdgeAsync(
            string modelId,
            string? modelVersion,
            IEnumerable<string> deviceIds,
            CancellationToken cancellationToken = default);
    }
}