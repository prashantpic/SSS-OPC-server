using AIService.Api.Dtos.PredictiveMaintenance;
// Assuming AIService.Application.PredictiveMaintenance.Commands.GetPredictionCommand exists
using AIService.Application.PredictiveMaintenance.Commands;
// Assuming AIService.Application.PredictiveMaintenance.Models.PredictionOutput exists
using AIService.Application.PredictiveMaintenance.Models;
using AIService.Grpc; // For gRPC types if needed
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using System.Linq;


namespace AIService.Api.Mappers
{
    public class PredictiveMaintenanceProfile : Profile
    {
        public PredictiveMaintenanceProfile()
        {
            // REST DTO to Application Command
            CreateMap<PredictionRequestDto, GetPredictionCommand>()
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.InputFeatures, opt => opt.MapFrom(src => src.InputFeatures));

            // Application Model to REST DTO
            CreateMap<PredictionOutput, PredictionResponseDto>()
                .ForMember(dest => dest.PredictionId, opt => opt.MapFrom(src => src.PredictionId))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => src.Results))
                .ForMember(dest => dest.ModelVersionUsed, opt => opt.MapFrom(src => src.ModelVersionUsed))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp));

            // Application Model to gRPC Response
            CreateMap<PredictionOutput, AIService.Grpc.PredictionResponse>()
                .ForMember(dest => dest.PredictionId, opt => opt.MapFrom(src => src.PredictionId))
                .ForMember(dest => dest.ModelVersionUsed, opt => opt.MapFrom(src => src.ModelVersionUsed))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.Timestamp.ToUniversalTime())))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => ConvertToProtoStruct(src.Results)));
        }
        
        private Struct ConvertToProtoStruct(System.Collections.Generic.IDictionary<string, object> dict)
        {
            var structProto = new Struct();
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    structProto.Fields.Add(kvp.Key, Value.ForObject(kvp.Value));
                }
            }
            return structProto;
        }
    }

    public static class ValueExtensions
    {
        public static Value ForObject(object obj)
        {
            if (obj == null) return Value.ForNull();
            if (obj is string s) return Value.ForString(s);
            if (obj is bool b) return Value.ForBool(b);
            if (obj is double d) return Value.ForNumber(d);
            if (obj is int i) return Value.ForNumber(i);
            if (obj is long l) return Value.ForNumber(l);
            if (obj is float f) return Value.ForNumber(f);
            // Add other numeric types as needed, ensuring conversion to double for Value.ForNumber
            if (obj is System.Collections.IDictionary idict) // Check for nested dictionary
            {
                var nestedStruct = new Struct();
                foreach(var key in idict.Keys)
                {
                    nestedStruct.Fields.Add(key.ToString(), ForObject(idict[key]));
                }
                return Value.ForStruct(nestedStruct);
            }
            if (obj is System.Collections.IEnumerable ienum && !(obj is string)) // Check for list/array
            {
                var listValue = new ListValue();
                foreach(var item in ienum)
                {
                    listValue.Values.Add(ForObject(item));
                }
                return Value.ForList(listValue);
            }


            // Fallback or throw exception for unsupported types
            return Value.ForString(obj.ToString()); 
        }
    }
}