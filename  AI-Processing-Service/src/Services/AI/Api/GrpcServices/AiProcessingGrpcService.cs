using Grpc.Core;
using AIService.Grpc; // Generated from AIService.proto
using MediatR;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AIService.Application.Interfaces; // For IModelManagementAppService, IEdgeDeploymentAppService
using AIService.Application.PredictiveMaintenance.Commands;
using AIService.Application.PredictiveMaintenance.Models;
using AIService.Application.AnomalyDetection.Commands;
using AIService.Application.AnomalyDetection.Models;
using AIService.Application.Nlq.Commands;
using AIService.Application.Nlq.Models;
using AIService.Application.ModelManagement.Commands; // For RegisterModelFeedbackCommand
using Google.Protobuf.WellKnownTypes; // For Timestamp, Struct
using System.IO; // For MemoryStream

namespace AIService.Api.GrpcServices
{
    public class AiProcessingGrpcService : AIService.Grpc.AIService.AIServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<AiProcessingGrpcService> _logger;
        private readonly IModelManagementAppService _modelManagementAppService;
        private readonly IEdgeDeploymentAppService _edgeDeploymentAppService;


        public AiProcessingGrpcService(
            IMediator mediator,
            IMapper mapper,
            ILogger<AiProcessingGrpcService> logger,
            IModelManagementAppService modelManagementAppService,
            IEdgeDeploymentAppService edgeDeploymentAppService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _modelManagementAppService = modelManagementAppService;
            _edgeDeploymentAppService = edgeDeploymentAppService;
        }

        public override async Task<PredictionResponse> GetPrediction(PredictionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetPrediction called for model ID: {ModelId}", request.ModelId);
            // Mapping from Grpc.PredictionRequest to Application.GetPredictionCommand
            // This mapping needs to be defined in an AutoMapper profile
            var command = _mapper.Map<GetPredictionCommand>(request);
            
            // var predictionOutput = await _mediator.Send(command);
            // Placeholder logic
            await Task.Delay(50);
             var predictionOutput = new PredictionOutput // This is a placeholder
            {
                PredictionId = System.Guid.NewGuid().ToString(),
                ModelIdUsed = request.ModelId ?? "default_pm_model_v1_grpc",
                Timestamp = System.DateTimeOffset.UtcNow,
                Results = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "RemainingUsefulLifeGrpc", 220.5 },
                    { "FailureProbabilityGrpc", 0.05 }
                }
            };

            if (predictionOutput == null)
            {
                _logger.LogError("gRPC GetPrediction returned null for model ID: {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, "Prediction processing failed."));
            }
            // Mapping from Application.PredictionOutput to Grpc.PredictionResponse
            return _mapper.Map<PredictionResponse>(predictionOutput);
        }

        public override async Task<AnomalyDetectionResponse> DetectAnomalies(AnomalyDetectionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DetectAnomalies called for model ID: {ModelId}", request.ModelId);
            var command = _mapper.Map<DetectAnomaliesCommand>(request);
            // var anomalyResult = await _mediator.Send(command);

            // Placeholder
            await Task.Delay(50);
            var anomalyResult = new AnomalyDetectionResult // Placeholder
            {
                ModelIdUsed = request.ModelId ?? "default_ad_model_v1_grpc",
                Timestamp = System.DateTimeOffset.UtcNow,
                DetectedAnomalies = new System.Collections.Generic.List<Application.AnomalyDetection.Models.AnomalyDetail>
                {
                    new Application.AnomalyDetection.Models.AnomalyDetail { Timestamp = System.DateTimeOffset.UtcNow.AddSeconds(-30), Score = 0.75, Description = "Anomaly via gRPC 1", Value = "75" },
                    new Application.AnomalyDetection.Models.AnomalyDetail { Timestamp = System.DateTimeOffset.UtcNow.AddSeconds(-10), Score = 0.95, Description = "Anomaly via gRPC 2", Value = "high_val" }
                }
            };

            if (anomalyResult == null)
            {
                 _logger.LogError("gRPC DetectAnomalies returned null for model ID: {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, "Anomaly detection processing failed."));
            }
            return _mapper.Map<AnomalyDetectionResponse>(anomalyResult);
        }

        public override async Task<NlqResponse> ProcessNlq(NlqRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC ProcessNlq called with query: {Query}", request.QueryText);
            var command = _mapper.Map<ProcessNlqCommand>(request);
            // var nlqResult = await _mediator.Send(command);

            // Placeholder
            await Task.Delay(50);
            var nlqResult = new NlqProcessingResult // Placeholder
            {
                OriginalQuery = request.QueryText,
                ProcessedQuery = request.QueryText.ToUpper(),
                Intent = "FindAssetStatus_gRPC",
                ConfidenceScore = 0.88,
                ResponseMessage = "Asset XYZ is Active (via gRPC).",
                Entities = new System.Collections.Generic.List<Application.Nlq.Models.NlqEntity>
                {
                    new Application.Nlq.Models.NlqEntity { Type = "Asset", Value = "XYZ" }
                }
            };

            if (nlqResult == null)
            {
                _logger.LogError("gRPC ProcessNlq returned null for query: {Query}", request.QueryText);
                throw new RpcException(new Status(StatusCode.Internal, "NLQ processing failed."));
            }
            return _mapper.Map<NlqResponse>(nlqResult);
        }

        public override async Task<EdgeDeploymentResponse> DeployModelToEdge(EdgeDeploymentRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DeployModelToEdge called for Model ID: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion);
            
            // Direct call to App Service or map to a command if preferred
            bool success = await _edgeDeploymentAppService.DeployModelAsync(
                request.ModelId,
                request.ModelVersion,
                request.TargetDeviceIds,
                request.DeploymentConfiguration
            );

            if (!success)
            {
                _logger.LogError("gRPC DeployModelToEdge failed for Model ID: {ModelId}", request.ModelId);
                 throw new RpcException(new Status(StatusCode.Internal, "Edge deployment initiation failed."));
            }

            return new EdgeDeploymentResponse
            {
                DeploymentId = System.Guid.NewGuid().ToString(), // Generate a tracking ID
                StatusMessage = "Edge deployment initiated successfully via gRPC.",
                Success = true
            };
        }
        
        public override async Task<ModelUploadResponse> UploadModel(ModelUploadRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC UploadModel called for model: {ModelName}, Version: {ModelVersion}", request.ModelName, request.ModelVersion);
            if (request.ModelFileContent.IsEmpty)
            {
                _logger.LogWarning("gRPC UploadModel called with empty file content for model: {ModelName}", request.ModelName);
                return new ModelUploadResponse { Success = false, Message = "Model file content cannot be empty." };
            }

            using var modelStream = new MemoryStream(request.ModelFileContent.ToByteArray());
            var modelId = await _modelManagementAppService.UploadModelAsync(
                request.ModelName,
                request.ModelVersion,
                request.ModelType,
                request.ModelFormat,
                request.Description,
                modelStream,
                request.FileName);

            if (string.IsNullOrEmpty(modelId))
            {
                _logger.LogError("gRPC UploadModel failed for model: {ModelName}", request.ModelName);
                return new ModelUploadResponse { Success = false, Message = "Model upload failed during processing.", ModelId = "" };
            }

            _logger.LogInformation("gRPC UploadModel successful for model: {ModelName}, ID: {ModelId}", request.ModelName, modelId);
            return new ModelUploadResponse { Success = true, Message = "Model uploaded successfully via gRPC.", ModelId = modelId };
        }

        public override async Task<ModelFeedbackResponse> SubmitModelFeedback(ModelFeedbackRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC SubmitModelFeedback called for model ID: {ModelId}", request.ModelId);
            
            var command = _mapper.Map<RegisterModelFeedbackCommand>(request);
            // var feedbackResult = await _mediator.Send(command); // Assuming command returns some result or throws

            // Placeholder for MediatR send
            await Task.Delay(10); // Simulate async work
            // bool success = feedbackResult.IsSuccess; // Example if command returns a result object

             _logger.LogInformation("gRPC SubmitModelFeedback processed for model ID: {ModelId}", request.ModelId);
            return new ModelFeedbackResponse
            {
                FeedbackId = System.Guid.NewGuid().ToString(),
                Message = "Feedback submitted successfully via gRPC.",
                Success = true // Assume success for now
            };
        }
    }
}