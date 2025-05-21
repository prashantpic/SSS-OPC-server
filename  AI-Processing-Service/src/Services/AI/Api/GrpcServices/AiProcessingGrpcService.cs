using AIService.Application.AnomalyDetection.Commands;
using AIService.Application.EdgeDeployment.Commands;
using AIService.Application.Nlq.Commands;
using AIService.Application.PredictiveMaintenance.Commands;
using AIService.Grpc; // Generated from .proto
using AutoMapper;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes; // For Timestamp and Struct

namespace AIService.Api.GrpcServices
{
    public class AiProcessingGrpcService : Grpc.AiProcessingService.AiProcessingServiceBase // Use generated base class
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<AiProcessingGrpcService> _logger;

        public AiProcessingGrpcService(IMediator mediator, IMapper mapper, ILogger<AiProcessingGrpcService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<GetPredictionResponse> GetPrediction(GetPredictionRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetPrediction request received for model {ModelId}", request.ModelId);
            try
            {
                // Manual mapping or AutoMapper profile for gRPC messages to Commands
                var command = new GetPredictionCommand
                {
                    ModelId = request.ModelId,
                    // Assuming InputData in command is Dictionary<string, object>
                    InputData = request.InputData.Fields.ToDictionary(f => f.Key, f => ConvertProtoValue(f.Value))
                };
                
                var result = await _mediator.Send(command);
                
                // Manual mapping or AutoMapper profile for Application Models to gRPC messages
                return new GetPredictionResponse
                {
                    PredictionId = result.PredictionId, // Assuming PredictionOutput has PredictionId
                    OutputData = ConvertDictionaryToProtoStruct(result.OutputData), // Assuming PredictionOutput has OutputData as Dictionary
                    StatusMessage = "Success" // Assuming PredictionOutput has Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gRPC GetPrediction request for model {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DetectAnomaliesResponse> DetectAnomalies(DetectAnomaliesRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DetectAnomalies request received for model {ModelId}", request.ModelId);
            try
            {
                var command = _mapper.Map<DetectAnomaliesCommand>(request); // Requires AutoMapper profile for gRPC types
                var result = await _mediator.Send(command);
                return _mapper.Map<DetectAnomaliesResponse>(result); // Requires AutoMapper profile for gRPC types
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gRPC DetectAnomalies request for model {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<ProcessNlqResponse> ProcessNlq(ProcessNlqRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC ProcessNlq request received for query: {Query}", request.QueryText);
            try
            {
                var command = _mapper.Map<ProcessNlqCommand>(request); // Requires AutoMapper profile
                var result = await _mediator.Send(command);
                return _mapper.Map<ProcessNlqResponse>(result); // Requires AutoMapper profile
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gRPC ProcessNlq request for query: {Query}", request.QueryText);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DeployModelToEdgeResponse> DeployModelToEdge(DeployModelToEdgeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC DeployModelToEdge request received for model {ModelId}", request.ModelId);
            try
            {
                var command = _mapper.Map<DeployModelToEdgeCommand>(request); // Requires AutoMapper profile
                var result = await _mediator.Send(command); // Assuming command returns a deployment ID string or object
                
                // Assuming result is a string (deploymentId)
                var deploymentId = result?.ToString() ?? "N/A";

                return new DeployModelToEdgeResponse
                {
                    DeploymentId = deploymentId,
                    StatusMessage = "Deployment initiated."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gRPC DeployModelToEdge request for model {ModelId}", request.ModelId);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
        
        // Helper to convert Protobuf Struct to Dictionary<string, object>
        private static System.Collections.Generic.Dictionary<string, object> ConvertProtoStructToDictionary(Struct s)
        {
            if (s == null) return new System.Collections.Generic.Dictionary<string, object>();
            return s.Fields.ToDictionary(kvp => kvp.Key, kvp => ConvertProtoValue(kvp.Value));
        }

        // Helper to convert Protobuf Value to .NET object
        private static object ConvertProtoValue(Value v)
        {
            switch (v.KindCase)
            {
                case Value.KindOneofCase.NullValue: return null;
                case Value.KindOneofCase.NumberValue: return v.NumberValue;
                case Value.KindOneofCase.StringValue: return v.StringValue;
                case Value.KindOneofCase.BoolValue: return v.BoolValue;
                case Value.KindOneofCase.StructValue: return ConvertProtoStructToDictionary(v.StructValue);
                case Value.KindOneofCase.ListValue: return v.ListValue.Values.Select(ConvertProtoValue).ToList();
                default: throw new ArgumentOutOfRangeException();
            }
        }

        // Helper to convert Dictionary<string, object> to Protobuf Struct
        private static Struct ConvertDictionaryToProtoStruct(System.Collections.Generic.IDictionary<string, object> dict)
        {
            if (dict == null) return new Struct();
            var s = new Struct();
            foreach (var kvp in dict)
            {
                s.Fields.Add(kvp.Key, ConvertNetObjectToProtoValue(kvp.Value));
            }
            return s;
        }
        
        // Helper to convert .NET object to Protobuf Value
        private static Value ConvertNetObjectToProtoValue(object obj)
        {
            if (obj == null) return Value.ForNull();
            if (obj is bool b) return Value.ForBool(b);
            if (obj is double d) return Value.ForNumber(d);
            if (obj is float f) return Value.ForNumber(f);
            if (obj is int i) return Value.ForNumber(i);
            if (obj is long l) return Value.ForNumber(l);
            if (obj is string s) return Value.ForString(s);
            if (obj is System.Collections.IDictionary idict) // Check for dictionary-like objects
            {
                var structVal = new Struct();
                foreach(System.Collections.DictionaryEntry entry in idict)
                {
                    if(entry.Key is string keyStr)
                    {
                         structVal.Fields.Add(keyStr, ConvertNetObjectToProtoValue(entry.Value));
                    }
                }
                return Value.ForStruct(structVal);
            }
            if (obj is System.Collections.IEnumerable ienum && !(obj is string)) // Check for list-like objects
            {
                var listVal = new ListValue();
                foreach(var item in ienum)
                {
                    listVal.Values.Add(ConvertNetObjectToProtoValue(item));
                }
                return Value.ForList(listVal);
            }
            // Fallback or throw exception for unsupported types
            return Value.ForString(obj.ToString());
        }
    }
}