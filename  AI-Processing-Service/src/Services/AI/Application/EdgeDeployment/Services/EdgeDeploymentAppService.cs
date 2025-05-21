using AIService.Application.EdgeDeployment.Interfaces;
using AIService.Application.EdgeDeployment.Models;
using AIService.Domain.Interfaces; // IModelRepository, IMlLopsClient
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

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
        private readonly IMlLopsClient _mlOpsClient; // MLOps platform often handles edge deployment orchestration
        private readonly ILogger<EdgeDeploymentAppService> _logger;
        private readonly IMapper _mapper;


        public EdgeDeploymentAppService(
            IModelRepository modelRepository,
            IMlLopsClient mlOpsClient,
            ILogger<EdgeDeploymentAppService> logger,
            IMapper mapper)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _mlOpsClient = mlOpsClient ?? throw new ArgumentNullException(nameof(mlOpsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<EdgeDeploymentResult> DeployModelToEdgeAsync(
            string modelId,
            string modelVersion,
            IEnumerable<string> edgeDeviceIds,
            EdgeDeploymentOptions options = null)
        {
            _logger.LogInformation("Initiating edge deployment for ModelId: {ModelId}, Version: {ModelVersion} to devices: {DeviceIds}",
                modelId, modelVersion, string.Join(",", edgeDeviceIds));

            if (edgeDeviceIds == null || !edgeDeviceIds.Any())
            {
                _logger.LogWarning("No edge device IDs provided for deployment of ModelId: {ModelId}", modelId);
                return new EdgeDeploymentResult { Success = false, Message = "No edge device IDs provided." };
            }

            try
            {
                // 1. Verify model exists (optional, MLOps client might do this)
                var model = await _modelRepository.GetModelAsync(modelId, modelVersion);
                if (model == null)
                {
                    _logger.LogError("ModelId: {ModelId}, Version: {ModelVersion} not found for edge deployment.", modelId, modelVersion);
                    return new EdgeDeploymentResult { Success = false, Message = $"Model {modelId} (Version: {modelVersion}) not found." };
                }

                // 2. Trigger deployment via MLOps client
                // The MLOps client should abstract the specifics of how deployment to edge occurs.
                // It might involve creating a deployment target, associating the model, etc.
                // REQ-8-001
                var deploymentTask = await _mlOpsClient.TriggerEdgeDeployment(modelId, modelVersion, edgeDeviceIds, options?.ToDictionary());

                _logger.LogInformation("Edge deployment initiated for ModelId: {ModelId} via MLOps. Task/Reference: {DeploymentTaskReference}", modelId, deploymentTask.DeploymentId);

                return new EdgeDeploymentResult
                {
                    Success = deploymentTask.Success,
                    Message = deploymentTask.Success ? "Edge deployment initiated successfully." : $"Failed to initiate edge deployment: {deploymentTask.Message}",
                    DeploymentReferenceId = deploymentTask.DeploymentId,
                    DeviceStatuses = deploymentTask.DeviceStatuses // Assuming MLOps client returns initial status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during edge deployment for ModelId: {ModelId}", modelId);
                return new EdgeDeploymentResult
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<EdgeDeviceDeploymentStatus>> GetEdgeDeploymentStatusAsync(
            string modelId,
            string modelVersion,
            IEnumerable<string> edgeDeviceIds = null)
        {
            _logger.LogInformation("Fetching edge deployment status for ModelId: {ModelId}, Version: {ModelVersion}, Devices: {DeviceIds}",
                modelId, modelVersion, edgeDeviceIds == null ? "All" : string.Join(",", edgeDeviceIds));

            try
            {
                var statuses = await _mlOpsClient.GetEdgeDeploymentStatusAsync(modelId, modelVersion, edgeDeviceIds);
                return _mapper.Map<IEnumerable<EdgeDeviceDeploymentStatus>>(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching edge deployment status for ModelId: {ModelId}", modelId);
                // Return empty or statuses indicating error
                if (edgeDeviceIds != null && edgeDeviceIds.Any())
                {
                    return edgeDeviceIds.Select(id => new EdgeDeviceDeploymentStatus
                    {
                        DeviceId = id,
                        ModelId = modelId,
                        ModelVersion = modelVersion,
                        Status = "Error",
                        Message = $"Failed to retrieve status: {ex.Message}"
                    }).ToList();
                }
                return Enumerable.Empty<EdgeDeviceDeploymentStatus>();
            }
        }
    }

    // Define placeholder models if not present in AIService.Application.EdgeDeployment.Models
    namespace AIService.Application.EdgeDeployment.Models
    {
        public class EdgeDeploymentOptions
        {
            public bool ForceRedeploy { get; set; }
            public Dictionary<string, string> AdditionalParameters { get; set; }

            public EdgeDeploymentOptions()
            {
                AdditionalParameters = new Dictionary<string, string>();
            }
            public Dictionary<string, string> ToDictionary()
            {
                var dict = new Dictionary<string, string>(AdditionalParameters);
                dict.Add(nameof(ForceRedeploy), ForceRedeploy.ToString());
                return dict;
            }
        }

        public class EdgeDeploymentResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string DeploymentReferenceId { get; set; } // ID from MLOps or deployment system
            public List<EdgeDeviceDeploymentStatus> DeviceStatuses { get; set; }
            public EdgeDeploymentResult()
            {
                DeviceStatuses = new List<EdgeDeviceDeploymentStatus>();
            }
        }

        public class EdgeDeviceDeploymentStatus
        {
            public string DeviceId { get; set; }
            public string ModelId { get; set; }
            public string ModelVersion { get; set; }
            public string Status { get; set; } // e.g., "Pending", "InProgress", "Succeeded", "Failed"
            public string Message { get; set; } // Optional message regarding status
            public DateTime LastUpdated { get; set; }
        }
        
        // From IMlLopsClient, for mapping
        public class MLOpsEdgeDeploymentTask
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string DeploymentId { get; set; }
            public List<EdgeDeviceDeploymentStatus> DeviceStatuses { get; set; }
        }
    }
}