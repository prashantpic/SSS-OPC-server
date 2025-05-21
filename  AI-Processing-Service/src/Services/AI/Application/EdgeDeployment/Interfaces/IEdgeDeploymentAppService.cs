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
        /// <param name="modelId">The ID of the AI model to deploy.</param>
        /// <param name="modelVersion">The version of the AI model to deploy.</param>
        /// <param name="edgeDeviceIds">A collection of unique identifiers for the target edge devices.</param>
        /// <param name="deploymentParameters">Additional parameters for the deployment (e.g., configuration overrides, scheduling).</param>
        /// <returns>A string identifier for the deployment task or operation, or a status object. 
        /// // TODO: Define a specific DeploymentInitiationResultDto
        /// </returns>
        Task<string> DeployModelAsync(
            string modelId,
            string modelVersion,
            IEnumerable<string> edgeDeviceIds,
            Dictionary<string, string> deploymentParameters);

        /// <summary>
        /// Retrieves the status of an ongoing or completed edge deployment.
        /// </summary>
        /// <param name="deploymentId">The unique identifier of the deployment task (returned by DeployModelAsync).</param>
        /// <returns>An object representing the deployment status. 
        /// // TODO: Define a specific EdgeDeploymentStatusDto
        /// </returns>
        Task<object> GetDeploymentStatusAsync(string deploymentId);

        /// <summary>
        /// Retrieves the deployment status for a specific model on a specific edge device.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="modelVersion">The version of the model.</param>
        /// <param name="edgeDeviceId">The ID of the edge device.</param>
        /// <returns>An object representing the deployment status for that specific model/device pair.
        /// // TODO: Define a specific EdgeDeploymentStatusDto
        /// </returns>
        Task<object> GetModelDeploymentStatusForDeviceAsync(string modelId, string modelVersion, string edgeDeviceId);

        /// <summary>
        /// Lists all models currently deployed or scheduled for deployment on a specific edge device.
        /// </summary>
        /// <param name="edgeDeviceId">The ID of the edge device.</param>
        /// <returns>A collection of objects representing deployed models.
        /// // TODO: Define a specific DeployedModelOnEdgeDto
        /// </returns>
        Task<IEnumerable<object>> ListDeployedModelsOnDeviceAsync(string edgeDeviceId);
    }
}