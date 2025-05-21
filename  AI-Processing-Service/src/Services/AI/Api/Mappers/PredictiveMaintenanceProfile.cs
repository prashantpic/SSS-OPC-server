using AutoMapper;
using AIService.Api.Dtos.PredictiveMaintenance;
using AIService.Application.PredictiveMaintenance.Commands; // Assuming GetPredictionCommand
using AIService.Application.PredictiveMaintenance.Models;   // Assuming PredictionOutput
using AIService.Grpc; // For gRPC types
using Google.Protobuf.WellKnownTypes; // For Struct, Timestamp
using System.Collections.Generic; // For Dictionary
using System; // For DateTimeOffset

namespace AIService.Api.Mappers
{
    public class PredictiveMaintenanceProfile : Profile
    {
        public PredictiveMaintenanceProfile()
        {
            // REST DTO to Application Command
            CreateMap<PredictionRequestDto, GetPredictionCommand>()
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.InputData, opt => opt.MapFrom(src => src.InputData));

            // Application Model to REST DTO
            CreateMap<PredictionOutput, PredictionResponseDto>()
                .ForMember(dest => dest.PredictionId, opt => opt.MapFrom(src => src.PredictionId))
                .ForMember(dest => dest.ModelIdUsed, opt => opt.MapFrom(src => src.ModelIdUsed))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => src.Results));

            // gRPC Request to Application Command
            CreateMap<AIService.Grpc.PredictionRequest, GetPredictionCommand>()
                .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.InputData, opt => opt.MapFrom(src => ConvertStructToDictionary(src.InputData)));

            // Application Model to gRPC Response
            CreateMap<PredictionOutput, AIService.Grpc.PredictionResponse>()
                .ForMember(dest => dest.PredictionId, opt => opt.MapFrom(src => src.PredictionId))
                .ForMember(dest => dest.ModelIdUsed, opt => opt.MapFrom(src => src.ModelIdUsed))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => Timestamp.FromDateTimeOffset(src.Timestamp)))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => ConvertDictionaryToStruct(src.Results)));
        }

        // Helper method to convert Google.Protobuf.Struct to Dictionary<string, object>
        private static Dictionary<string, object> ConvertStructToDictionary(Struct s)
        {
            if (s == null) return null;
            var dict = new Dictionary<string, object>();
            foreach (var (key, value) in s.Fields)
            {
                dict[key] = ConvertProtoValueToObject(value);
            }
            return dict;
        }

        // Helper method to convert Dictionary<string, object> to Google.Protobuf.Struct
        private static Struct ConvertDictionaryToStruct(Dictionary<string, object> dict)
        {
            if (dict == null) return new Struct();
            var s = new Struct();
            foreach (var (key, value) in dict)
            {
                s.Fields[key] = ConvertObjectToProtoValue(value);
            }
            return s;
        }
        
        private static object ConvertProtoValueToObject(Value protoValue)
        {
            if (protoValue == null) return null;
            switch (protoValue.KindCase)
            {
                case Value.KindOneofCase.NullValue:
                    return null;
                case Value.KindOneofCase.NumberValue:
                    return protoValue.NumberValue;
                case Value.KindOneofCase.StringValue:
                    return protoValue.StringValue;
                case Value.KindOneofCase.BoolValue:
                    return protoValue.BoolValue;
                case Value.KindOneofCase.StructValue:
                    return ConvertStructToDictionary(protoValue.StructValue);
                case Value.KindOneofCase.ListValue:
                    var list = new List<object>();
                    foreach (var item in protoValue.ListValue.Values)
                    {
                        list.Add(ConvertProtoValueToObject(item));
                    }
                    return list;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported Value.KindCase: {protoValue.KindCase}");
            }
        }

        private static Value ConvertObjectToProtoValue(object obj)
        {
            if (obj == null) return Value.ForNull();

            switch (obj)
            {
                case int i: return Value.ForNumber(i);
                case long l: return Value.ForNumber(l);
                case float f: return Value.ForNumber(f);
                case double d: return Value.ForNumber(d);
                case decimal dec: return Value.ForNumber((double)dec); // Potential precision loss
                case string s: return Value.ForString(s);
                case bool b: return Value.ForBool(b);
                case DateTime dt: return Value.ForString(dt.ToString("o")); // ISO 8601
                case DateTimeOffset dto: return Value.ForString(dto.ToString("o")); // ISO 8601
                case IDictionary<string, object> dict:
                    var structValue = new Struct();
                    foreach (var kvp in dict)
                    {
                        structValue.Fields.Add(kvp.Key, ConvertObjectToProtoValue(kvp.Value));
                    }
                    return Value.ForStruct(structValue);
                case IEnumerable<object> list:
                    var listValue = new ListValue();
                    foreach (var item in list)
                    {
                        listValue.Values.Add(ConvertObjectToProtoValue(item));
                    }
                    return Value.ForList(listValue);
                // Add other types as needed
                default:
                    return Value.ForString(obj.ToString()); // Fallback to string
            }
        }
    }
}