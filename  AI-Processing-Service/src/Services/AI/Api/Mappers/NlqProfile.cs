using AutoMapper;
using AIService.Api.Dtos.Nlq;
using AIService.Application.Nlq.Commands; // Assuming ProcessNlqCommand
using AIService.Application.Nlq.Models;   // Assuming NlqProcessingResult and NlqEntity (application model)
using AIService.Grpc; // For gRPC types
using System.Linq; // For Select

namespace AIService.Api.Mappers
{
    public class NlqProfile : Profile
    {
        public NlqProfile()
        {
            // REST DTO to Application Command
            CreateMap<NlqRequestDto, ProcessNlqCommand>()
                .ForMember(dest => dest.QueryText, opt => opt.MapFrom(src => src.QueryText))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.ContextData, opt => opt.MapFrom(src => src.ContextData));

            // Application Model (NlqEntity) to REST DTO (NlqEntityDto)
            CreateMap<AIService.Application.Nlq.Models.NlqEntity, NlqEntityDto>();

            // Application Model (NlqProcessingResult) to REST DTO (NlqResponseDto)
            CreateMap<NlqProcessingResult, NlqResponseDto>()
                .ForMember(dest => dest.OriginalQuery, opt => opt.MapFrom(src => src.OriginalQuery))
                .ForMember(dest => dest.ProcessedQuery, opt => opt.MapFrom(src => src.ProcessedQuery))
                .ForMember(dest => dest.Intent, opt => opt.MapFrom(src => src.Intent))
                .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.Entities)) // Assumes NlqEntity mapping is defined
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.ConfidenceScore))
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.ResponseMessage))
                .ForMember(dest => dest.DataPayload, opt => opt.MapFrom(src => src.DataPayload)); // Assuming DataPayload is object

            // gRPC Request to Application Command
            CreateMap<AIService.Grpc.NlqRequest, ProcessNlqCommand>()
                .ForMember(dest => dest.QueryText, opt => opt.MapFrom(src => src.QueryText))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.ContextData, opt => opt.MapFrom(src => src.ContextData.ToDictionary(kv => kv.Key, kv => kv.Value)));

            // Application Model (NlqEntity) to gRPC (NlqEntity)
            CreateMap<AIService.Application.Nlq.Models.NlqEntity, AIService.Grpc.NlqEntity>();

            // Application Model (NlqProcessingResult) to gRPC Response
            CreateMap<NlqProcessingResult, AIService.Grpc.NlqResponse>()
                .ForMember(dest => dest.OriginalQuery, opt => opt.MapFrom(src => src.OriginalQuery))
                .ForMember(dest => dest.ProcessedQuery, opt => opt.MapFrom(src => src.ProcessedQuery))
                .ForMember(dest => dest.Intent, opt => opt.MapFrom(src => src.Intent))
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.ConfidenceScore))
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.ResponseMessage))
                .AfterMap((src, dest, context) => { // Handle collection mapping for Entities
                    if (src.Entities != null)
                    {
                        foreach (var entity in src.Entities)
                        {
                            dest.Entities.Add(context.Mapper.Map<AIService.Grpc.NlqEntity>(entity));
                        }
                    }
                });
                // Note: DataPayload mapping to gRPC would require specific handling,
                // perhaps using google.protobuf.Any or google.protobuf.Struct if it's generic.
                // For simplicity, it's omitted here; assume ResponseMessage covers most gRPC cases.
        }
    }
}