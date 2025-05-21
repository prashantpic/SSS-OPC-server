using AIService.Application.Interfaces;
// Placeholder for Application layer commands and models
// using AIService.Application.PredictiveMaintenance.Commands;
// using AIService.Application.PredictiveMaintenance.Models;
// using AIService.Application.AnomalyDetection.Commands;
// using AIService.Application.AnomalyDetection.Models;
// using AIService.Application.Nlq.Commands;
// using AIService.Application.Nlq.Models;
// using AIService.Application.ModelManagement.Commands;
// using AIService.Application.ModelManagement.Models;
// using AIService.Application.EdgeDeployment.Models;
using AIService.Grpc; // Generated from AIService.proto
using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Api.GrpcServices
{
    public class AiProcessingGrpcService : AIService.Grpc.AIService.AIServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IModelManagementAppService _modelManagementAppService;
        private readonly IEdgeDeploymentAppService _edgeDeploymentAppService;
        private readonly ILogger<AiProcessingGrpcService> _logger;

        public AiProcessingGrpcService(
            IMediator mediator,
            IMapper mapper,
            IModelManagementAppService modelManagementAppService,
            IEdgeDeploymentAppService edgeDeploymentAppService,
            ILogger<AiProcessingGrpcService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _modelManagementAppService = modelManagementAppService ?? throw new ArgumentNullException(nameof(modelManagementAppService));
            _edgeDeploymentAppService = edgeDeploymentAppService ?? throw new ArgumentNullException(nameof(edgeDeploymentAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<PredictionResponse> GetPrediction(PredictionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetPrediction called for model {ModelId}", request.ModelId);
            try
            {
                var command = new AIService.Application.PredictiveMaintenance.Commands.GetPredictionCommand
                {
                    ModelId = request.ModelId,
                    InputFeatures = request.InputFeatures?.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value))
                };

                var result = await _mediator.Send(command);
                if (result == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Prediction could not be generated."));
                }
                
                return _mapper.Map<PredictionResponse>(result); // Assumes mapping PredictionOutput to PredictionResponse
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC GetPrediction for model {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<AnomalyDetectionResponse> DetectAnomalies(AnomalyDetectionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DetectAnomalies called for model {ModelId}", request.ModelId);
            try
            {
                var command = new AIService.Application.AnomalyDetection.Commands.DetectAnomaliesCommand
                {
                    ModelId = request.ModelId,
                    DataPoints = request.DataPoints.Select(dp => 
                        dp.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value))
                    ).ToList()
                };

                var result = await _mediator.Send(command); // Assuming this returns AnomalyDetectionOutput
                if (result == null)
                {
                     throw new RpcException(new Status(StatusCode.NotFound, "Anomaly detection yielded no result."));
                }
                // Manual mapping or AutoMapper profile required here
                // For now, let's assume a mapper profile `AnomalyDetectionOutput` -> `AnomalyDetectionResponse` exists.
                // If not, a manual mapping would be needed:
                var response = _mapper.Map<AnomalyDetectionResponse>(result);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC DetectAnomalies for model {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<NlqResponse> ProcessNlq(NlqRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC ProcessNlq called for query: {QueryText}", request.QueryText);
            try
            {
                var command = new AIService.Application.Nlq.Commands.ProcessNlqCommand
                {
                    QueryText = request.QueryText,
                    UserId = request.UserId,
                    SessionId = request.SessionId,
                    ContextParameters = request.ContextParameters.ToDictionary(kv => kv.Key, kv => kv.Value)
                };
                var result = await _mediator.Send(command); // Assuming this returns NlqProcessingResult
                 if (result == null)
                {
                     throw new RpcException(new Status(StatusCode.NotFound, "NLQ processing yielded no result."));
                }
                return _mapper.Map<NlqResponse>(result); // Assumes mapping NlqProcessingResult to NlqResponse
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC ProcessNlq for query: {QueryText}", request.QueryText);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<EdgeDeploymentResponse> DeployModelToEdge(EdgeDeploymentRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DeployModelToEdge called for Model ID {ModelId} to devices {DeviceIds}", request.ModelId, string.Join(",", request.TargetDeviceIds));
            try
            {
                 var appServiceRequest = new AIService.Application.EdgeDeployment.Models.EdgeDeploymentAppServiceRequest
                {
                    ModelId = request.ModelId,
                    ModelVersion = request.ModelVersion,
                    TargetDeviceIds = request.TargetDeviceIds.ToList(),
                    DeploymentConfiguration = request.DeploymentConfiguration.ToDictionary(kv => kv.Key, kv => kv.Value)
                };
                var result = await _edgeDeploymentAppService.DeployModelAsync(appServiceRequest); // Returns EdgeDeploymentResultDto
                
                var response = new EdgeDeploymentResponse { Success = result.Success, DeploymentId = result.DeploymentId, Message = result.Message };
                foreach(var item in result.DeviceDeploymentStatus) { response.DeviceDeploymentStatus.Add(item.Key, item.Value); }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC DeployModelToEdge for Model ID {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<ModelUploadResponse> UploadModel(IAsyncStreamReader<ModelUploadRequestChunk> requestStream, ServerCallContext context)
        {
            _logger.LogInformation("gRPC UploadModel stream opened.");
            AIService.Application.ModelManagement.Models.ModelUploadRequest appServiceRequest = null;
            MemoryStream modelFileStream = new MemoryStream();

            try
            {
                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (chunk.ChunkDataCase == ModelUploadRequestChunk.ChunkDataOneofCase.Metadata)
                    {
                        if (appServiceRequest != null)
                             throw new RpcException(new Status(StatusCode.InvalidArgument, "Metadata chunk received more than once."));
                        
                        _logger.LogInformation("Received metadata for model: {ModelName}", chunk.Metadata.Name);
                        appServiceRequest = new AIService.Application.ModelManagement.Models.ModelUploadRequest
                        {
                            Name = chunk.Metadata.Name,
                            Version = chunk.Metadata.Version,
                            Description = chunk.Metadata.Description,
                            ModelType = chunk.Metadata.ModelType,
                            ModelFormat = chunk.Metadata.ModelFormat,
                            FileName = chunk.Metadata.FileName,
                            InputSchema = chunk.Metadata.InputSchema,
                            OutputSchema = chunk.Metadata.OutputSchema
                        };
                    }
                    else if (chunk.ChunkDataCase == ModelUploadRequestChunk.ChunkDataOneofCase.FileChunk)
                    {
                        if (appServiceRequest == null)
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "File chunk received before metadata."));
                        
                        await modelFileStream.WriteAsync(chunk.FileChunk.Content.ToByteArray());
                    }
                }

                if (appServiceRequest == null)
                     throw new RpcException(new Status(StatusCode.InvalidArgument, "No metadata received for model upload."));
                if (modelFileStream.Length == 0)
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "No file content received for model upload."));

                modelFileStream.Seek(0, SeekOrigin.Begin);
                appServiceRequest.ModelFileStream = modelFileStream;

                var result = await _modelManagementAppService.UploadModelAsync(appServiceRequest);
                return new ModelUploadResponse { Success = result.Success, ModelId = result.ModelId, Message = result.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC UploadModel.");
                throw new RpcException(new Status(StatusCode.Internal, $"Error uploading model: {ex.Message}"));
            }
            finally
            {
                await modelFileStream.DisposeAsync();
            }
        }

        public override async Task<ModelFeedbackResponse> RegisterModelFeedback(ModelFeedbackRequest request, ServerCallContext context)
        {
             _logger.LogInformation("gRPC RegisterModelFeedback called for Model ID {ModelId}", request.ModelId);
            try
            {
                 var command = new AIService.Application.ModelManagement.Commands.RegisterModelFeedbackCommand
                {
                    ModelId = request.ModelId,
                    ModelVersion = request.ModelVersion,
                    PredictionId = request.PredictionId,
                    InputFeatures = request.InputFeatures?.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value)),
                    ActualOutcome = request.ActualOutcome?.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value)),
                    FeedbackText = request.FeedbackText,
                    IsCorrectPrediction = request.IsCorrectPrediction,
                    UserId = request.UserId,
                    Timestamp = request.Timestamp.ToDateTimeOffset()
                };
                await _mediator.Send(command);
                return new ModelFeedbackResponse { Success = true, Message = "Feedback registered successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gRPC RegisterModelFeedback for Model ID {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        // Helper to convert Protobuf Value to .NET object
        private object ConvertProtoValue(Google.Protobuf.WellKnownTypes.Value protoValue)
        {
            switch (protoValue.KindCase)
            {
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue:
                    return null;
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue:
                    return protoValue.NumberValue;
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue:
                    return protoValue.StringValue;
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.BoolValue:
                    return protoValue.BoolValue;
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StructValue:
                    return protoValue.StructValue.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value));
                case Google.Protobuf.WellKnownTypes.Value.KindOneofCase.ListValue:
                    return protoValue.ListValue.Values.Select(ConvertProtoValue).ToList();
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported Protobuf Value kind: {protoValue.KindCase}");
            }
        }
    }
}