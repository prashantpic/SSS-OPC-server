using AIService.Application.EdgeDeployment.Interfaces;
using AIService.Application.EdgeDeployment.Models.Results;
using AIService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.EdgeDeployment.Services
{
    /// <summary>
    /// Handles the logic for deploying specified AI models to target edge devices.
    /// Interacts with MLOps platforms or specific edge management tools via IMlLopsClient.
    /// REQ-8-001: Edge AI Model Deployment.
    /// </summary>
    public class EdgeDeploymentAppService : IEdgeDeploymentAppService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IMlLopsClient _mlOpsClient;
        private readonly ILogger<EdgeDeploymentAppService> _logger;

        public EdgeDeploymentAppService(
            IModelRepository modelRepository,
            IMlLopsClient mlOpsClient,
            ILogger<EdgeDeploymentAppService> logger)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _mlOpsClient = mlOpsClient ?? throw new ArgumentNullException(nameof(mlOpsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EdgeDeploymentOperationResult> DeployModelToEdgeAsync(
            string modelId,
            string? modelVersion,
            IEnumerable<string> deviceIds,
            Dictionary<string, string>? deploymentConfiguration,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to deploy ModelId: {ModelId}, Version: {ModelVersion} to {DeviceCount} devices.", modelId, modelVersion ?? "Latest", deviceIds.Count());

            if (!deviceIds.Any())
            {
                _logger.LogWarning("No device IDs provided for edge deployment of ModelId: {ModelId}", modelId);
                return new EdgeDeploymentOperationResult { Success = false, Message = "No device IDs provided." };
            }

            try
            {
                // 1. Verify model existence and retrieve necessary details (e.g., artifact path/ID)
                var model = await _modelRepository.GetModelAsync(modelId, modelVersion, cancellationToken);
                if (model == null)
                {
                    _logger.LogError("ModelId: {ModelId}, Version: {ModelVersion} not found for edge deployment.", modelId, modelVersion ?? "N/A");
                    return new EdgeDeploymentOperationResult { Success = false, Message = "Model not found." };
                }

                // 2. Interact with MLOps client to trigger the edge deployment
                // The IMlLopsClient is responsible for the specifics of how deployment is triggered.
                var deploymentResult = await _mlOpsClient.TriggerEdgeDeploymentAsync(
                    model.Id,
                    model.Version, // Use resolved version from repository
                    model.StorageReference, // Pass storage reference or artifact path
                    model.ModelFormat,
                    deviceIds,
                    deploymentConfiguration,
                    cancellationToken);
                
                if (deploymentResult.Success)
                {
                    _logger.LogInformation("Edge deployment initiated for ModelId: {ModelId} to devices. Deployment ID: {DeploymentId}", modelId, deploymentResult.DeploymentId);
                }
                else
                {
                    _logger.LogWarning("Edge deployment initiation failed for ModelId: {ModelId}. Reason: {Reason}", modelId, deploymentResult.Message);
                }

                return new EdgeDeploymentOperationResult
                {
                    Success = deploymentResult.Success,
                    Message = deploymentResult.Message ?? "Deployment initiation processed.",
                    DeploymentId = deploymentResult.DeploymentId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during edge deployment for ModelId: {ModelId}", modelId);
                return new EdgeDeploymentOperationResult { Success = false, Message = $"Error during edge deployment: {ex.Message}" };
            }
        }

        public async Task<EdgeDeploymentStatusInfo> GetEdgeDeploymentStatusAsync(string deploymentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting edge deployment status for DeploymentId: {DeploymentId}", deploymentId);
            try
            {
                var status = await _mlOpsClient.GetEdgeDeploymentStatusAsync(deploymentId, cancellationToken);
                if (status == null)
                {
                    _logger.LogWarning("No status found for edge deployment ID: {DeploymentId}", deploymentId);
                    return new EdgeDeploymentStatusInfo { Success = false, Message = "Deployment status not found." };
                }
                
                return new EdgeDeploymentStatusInfo {
                    Success = true,
                    DeploymentId = status.DeploymentId,
                    ModelId = status.ModelId,
                    ModelVersion = status.ModelVersion,
                    OverallStatus = status.OverallStatus,
                    DeviceStatuses = status.DeviceStatuses ?? new Dictionary<string, string>(),
                    Message = "Status retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edge deployment status for DeploymentId: {DeploymentId}", deploymentId);
                return new EdgeDeploymentStatusInfo { Success = false, Message = $"Error getting status: {ex.Message}" };
            }
        }
        
        public async Task<IEnumerable<EdgeModelInfo>> GetModelsForDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting models for DeviceId: {DeviceId}", deviceId);
            try
            {
                var models = await _mlOpsClient.ListModelsDeployedOnEdgeDeviceAsync(deviceId, cancellationToken);
                 // This might require mapping from whatever MLOps client returns to EdgeModelInfo
                return models.Select(m => new EdgeModelInfo {
                    ModelId = m.ModelId,
                    Version = m.Version,
                    Format = m.Format.ToString(), // Assuming MLOps model info has format
                    DeploymentStatus = m.Status, // Assuming MLOps model info has status on device
                    DeployedAt = m.DeployedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models for DeviceId: {DeviceId}", deviceId);
                return Enumerable.Empty<EdgeModelInfo>();
            }
        }

        public async Task<EdgeDeploymentOperationResult> RecallModelFromEdgeAsync(
            string modelId,
            string? modelVersion,
            IEnumerable<string> deviceIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to recall ModelId: {ModelId}, Version: {ModelVersion} from {DeviceCount} devices.", modelId, modelVersion ?? "Latest", deviceIds.Count());
             if (!deviceIds.Any())
            {
                _logger.LogWarning("No device IDs provided for recall of ModelId: {ModelId}", modelId);
                return new EdgeDeploymentOperationResult { Success = false, Message = "No device IDs provided for recall." };
            }
            try
            {
                var model = await _modelRepository.GetModelAsync(modelId, modelVersion, cancellationToken);
                if (model == null)
                {
                    _logger.LogError("ModelId: {ModelId}, Version: {ModelVersion} not found for recall operation.", modelId, modelVersion ?? "N/A");
                    return new EdgeDeploymentOperationResult { Success = false, Message = "Model not found." };
                }

                var recallResult = await _mlOpsClient.TriggerEdgeModelRecallAsync(
                    model.Id,
                    model.Version,
                    deviceIds,
                    cancellationToken);

                if (recallResult.Success)
                {
                    _logger.LogInformation("Edge model recall initiated for ModelId: {ModelId}. Recall Operation ID: {DeploymentId}", modelId, recallResult.DeploymentId);
                }
                else
                {
                    _logger.LogWarning("Edge model recall initiation failed for ModelId: {ModelId}. Reason: {Reason}", modelId, recallResult.Message);
                }
                
                return new EdgeDeploymentOperationResult
                {
                    Success = recallResult.Success,
                    Message = recallResult.Message ?? "Recall initiation processed.",
                    DeploymentId = recallResult.DeploymentId // This might be a "RecallOperationId"
                };
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error during edge model recall for ModelId: {ModelId}", modelId);
                return new EdgeDeploymentOperationResult { Success = false, Message = $"Error during edge model recall: {ex.Message}" };
            }
        }
    }
}