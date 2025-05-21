using AIService.Api.Dtos.PredictiveMaintenance;
using AIService.Application.PredictiveMaintenance.Commands; // Assuming this namespace
using AIService.Application.PredictiveMaintenance.Models;   // Assuming this namespace
using AIService.Grpc; // For gRPC types if needed
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using System.Collections.Generic; // Required for IDictionary

namespace AIService.Api.Mappers
{
    public class PredictiveMaintenanceProfile : Profile
    {
        public PredictiveMaintenanceProfile()
        {
            // REST DTOs to Application Commands/Models
            CreateMap<PredictionRequestDto, GetPredictionCommand>();
            CreateMap<PredictionResult, PredictionResponseDto>();
                // .ForMember(dest => dest.RemainingUsefulLife, opt => opt.MapFrom(src => src.OutputData.ContainsKey("RUL") ? (double?)src.OutputData["RUL"] : null))
                // .ForMember(dest => dest.FailureProbability, opt => opt.MapFrom(src => src.OutputData.ContainsKey("Probability") ? (double?)src.OutputData["Probability"] : null));


            // gRPC Messages to Application Commands/Models
            CreateMap<GetPredictionRequest, GetPredictionCommand>()
                .ForMember(dest => dest.InputData, opt => opt.MapFrom(src => ConvertProtoStructToDictionary(src.InputData)));

            CreateMap<PredictionResult, GetPredictionResponse>()
                .ForMember(dest => dest.PredictionId, opt => opt.MapFrom(src => src.PredictionId))
                .ForMember(dest => dest.OutputData, opt => opt.MapFrom(src => ConvertDictionaryToProtoStruct(src.OutputData)))
                .ForMember(dest => dest.StatusMessage, opt => opt.MapFrom(src => src.Status));
        }

        private static Dictionary<string, object> ConvertProtoStructToDictionary(Struct s)
        {
            if (s == null) return new Dictionary<string, object>();
            return s.Fields.ToDictionary(kvp => kvp.Key, kvp => ConvertProtoValue(kvp.Value));
        }

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
                default: throw new System.ArgumentOutOfRangeException();
            }
        }
         private static Struct ConvertDictionaryToProtoStruct(IDictionary<string, object> dict)
        {
            if (dict == null) return new Struct();
            var s = new Struct();
            foreach (var kvp in dict)
            {
                s.Fields.Add(kvp.Key, ConvertNetObjectToProtoValue(kvp.Value));
            }
            return s;
        }
        
        private static Value ConvertNetObjectToProtoValue(object obj)
        {
            if (obj == null) return Value.ForNull();
            if (obj is bool b) return Value.ForBool(b);
            if (obj is double d) return Value.ForNumber(d);
            if (obj is float f) return Value.ForNumber(f);
            if (obj is int i) return Value.ForNumber(i);
            if (obj is long l) return Value.ForNumber(l);
            if (obj is string s) return Value.ForString(s);
            if (obj is IDictionary<string, object> idictStrObj) return Value.ForStruct(ConvertDictionaryToProtoStruct(idictStrObj));
            if (obj is System.Collections.IDictionary idict)
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
            if (obj is System.Collections.IEnumerable ienum && !(obj is string))
            {
                var listVal = new ListValue();
                foreach(var item in ienum)
                {
                    listVal.Values.Add(ConvertNetObjectToProtoValue(item));
                }
                return Value.ForList(listVal);
            }
            return Value.ForString(obj.ToString()); // Fallback
        }
    }
}