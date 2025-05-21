using AIService.Application.EdgeDeployment.Interfaces;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Application.EdgeDeployment.Services
{
    /// <summary>
    /// Handles the logic for deploying specified AI models to target edge devices,
    /// potentially interacting with MLOps platforms or specific edge management tools.
    /// REQ-8-001: Edge AI Model Deployment
    /// </summary>
    public class EdgeDeploymentAppService : IEdgeDeploymentAppService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IMlLopsClient _mlOpsClient; // MLOps client often handles edge deployment orchestration
        // private readonly IEdgeManagementClient _edgeManagementClient; // Or a dedicated client for edge platform
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

        public async Task<string> DeployModelAsync(
            string modelId,
            string modelVersion,
            IEnumerable<string> edgeDeviceIds,
            Dictionary<string, string> deploymentParameters)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                throw new ArgumentException("ModelId cannot be null or whitespace.", nameof(modelId));
            if (edgeDeviceIds == null || !edgeDeviceIds.Any())
                throw new ArgumentException("EdgeDeviceIds cannot be null or empty.", nameof(edgeDeviceIds));

            _logger.LogInformation("Initiating deployment of ModelId: {ModelId}, Version: {ModelVersion} to {DeviceCount} edge devices.",
                modelId, modelVersion, edgeDeviceIds.Count());

            // 1. Retrieve AiModel details to ensure it exists and is suitable for edge.
            var aiModel = await _modelRepository.GetModelAsync(modelId, modelVersion);
            if (aiModel == null)
            {
                _logger.LogError("Model with Id: {ModelId}, Version: {ModelVersion} not found for edge deployment.", modelId, modelVersion);
                throw new KeyNotFoundException($"Model with Id: {modelId}, Version: {modelVersion} not found.");
            }

            // Check if model format is suitable for edge (e.g., ONNX, TensorFlowLite)
            // This logic might be more complex based on specific edge capabilities.
            if (aiModel.ModelFormat != Domain.Enums.ModelFormat.ONNX &&
                aiModel.ModelFormat != Domain.Enums.ModelFormat.TensorFlowLite &&
                aiModel.ModelFormat != Domain.Enums.ModelFormat.MLNetZip) // ML.NET can also run on edge
            {
                _logger.LogError("Model format {ModelFormat} is not typically suitable for edge deployment. ModelId: {ModelId}", aiModel.ModelFormat, modelId);
                // Depending on strictness, this could throw an error or just be a warning.
                // For now, we proceed, assuming MLOps/Edge client handles compatibility.
            }

            // 2. Interact with MLOps client or Edge Management client to trigger deployment (REQ-8-001)
            // The MLOps client is expected to handle the complexities of packaging and distributing the model.
            var deploymentId = await _mlOpsClient.TriggerEdgeDeploymentAsync(aiModel, edgeDeviceIds, deploymentParameters);

            _logger.LogInformation("Edge deployment initiated for ModelId: {ModelId}. Deployment ID: {DeploymentId}", modelId, deploymentId);
            return deploymentId; // This ID can be used to track deployment status.
        }

        public async Task<object> GetDeploymentStatusAsync(string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(deploymentId))
                throw new ArgumentException("DeploymentId cannot be null or whitespace.", nameof(deploymentId));

            _logger.LogInformation("Getting edge deployment status for DeploymentId: {DeploymentId}", deploymentId);
            var status = await _mlOpsClient.GetEdgeDeploymentStatusAsync(deploymentId);
            // TODO: Map status to a defined EdgeDeploymentStatusDto
            _logger.LogInformation("Retrieved status for DeploymentId: {DeploymentId}: {@Status}", deploymentId, status);
            return status;
        }

        public async Task<object> GetModelDeploymentStatusForDeviceAsync(string modelId, string modelVersion, string edgeDeviceId)
        {
            if (string.IsNullOrWhiteSpace(modelId) || string.IsNullOrWhiteSpace(edgeDeviceId))
                throw new ArgumentException("ModelId, and EdgeDeviceId cannot be null or whitespace.");
            
            _logger.LogInformation("Getting deployment status for ModelId: {ModelId}, Version: {ModelVersion} on EdgeDevice: {EdgeDeviceId}",
                modelId, modelVersion, edgeDeviceId);
            
            // This might be a more specific query to the MLOps platform or edge management system
            var status = await _mlOpsClient.GetSpecificEdgeModelStatusAsync(modelId, modelVersion, edgeDeviceId);
             _logger.LogInformation("Retrieved status for ModelId: {ModelId} on EdgeDevice: {EdgeDeviceId}: {@Status}", modelId, edgeDeviceId, status);
            return status; // TODO: Map to DTO
        }

        public async Task<IEnumerable<object>> ListDeployedModelsOnDeviceAsync(string edgeDeviceId)
        {
             if (string.IsNullOrWhiteSpace(edgeDeviceId))
                throw new ArgumentException("EdgeDeviceId cannot be null or whitespace.");

            _logger.LogInformation("Listing deployed models on EdgeDevice: {EdgeDeviceId}", edgeDeviceId);
            var deployedModels = await _mlOpsClient.ListModelsOnEdgeDeviceAsync(edgeDeviceId);
             _logger.LogInformation("Retrieved {Count} deployed models for EdgeDevice: {EdgeDeviceId}", deployedModels.Count(), edgeDeviceId);
            return deployedModels; // TODO: Map to DTOs
        }
    }
}