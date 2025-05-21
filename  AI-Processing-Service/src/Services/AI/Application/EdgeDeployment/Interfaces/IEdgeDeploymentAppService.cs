using AIService.Application.EdgeDeployment.Models; // Assuming this namespace for custom models
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIService.Application.EdgeDeployment.Interfaces
{
    /// <summary>
    /// Defines contracts for orchestrating the deployment of AI models to edge devices.
    /// REQ-8-001: Edge AI Model Deployment
    /// </summary>
    public interface IEdgeDeploymentAppService
    {
        /// <summary>
        /// Initiates the deployment of a specified AI model to one or more edge devices.
        /// </summary>
        /// <param name="modelId">The ID of the model to deploy.</param>
        /// <param name="modelVersion">The version of the model to deploy.</param>
        /// <param name="edgeDeviceIds">A collection of identifiers for the target edge devices.</param>
        /// <param name="options">Optional deployment configurations.</param>
        /// <returns>A result object indicating the status of the deployment initiation.</returns>
        Task<EdgeDeploymentResult> DeployModelToEdgeAsync(string modelId, string modelVersion, IEnumerable<string> edgeDeviceIds, EdgeDeploymentOptions options = null);

        /// <summary>
        /// Retrieves the deployment status of a model on specific edge devices.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="modelVersion">The version of the model.</param>
        /// <param name="edgeDeviceIds">A collection of identifiers for the target edge devices. If null or empty, might query all known devices for this model.</param>
        /// <returns>A collection of deployment statuses for each queried device.</returns>
        Task<IEnumerable<EdgeDeviceDeploymentStatus>> GetEdgeDeploymentStatusAsync(string modelId, string modelVersion, IEnumerable<string> edgeDeviceIds = null);
    }
}