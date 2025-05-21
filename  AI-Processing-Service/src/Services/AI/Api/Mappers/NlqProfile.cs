using AIService.Api.Dtos.Nlq;
// Assuming AIService.Application.Nlq.Commands.ProcessNlqCommand exists
using AIService.Application.Nlq.Commands;
// Assuming AIService.Application.Nlq.Models.NlqProcessingResult and NlqEntity exist
using AIService.Application.Nlq.Models;
using AIService.Grpc; // For gRPC types
using AutoMapper;
using System.Linq;

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
                .ForMember(dest => dest.ContextParameters, opt => opt.MapFrom(src => src.ContextParameters));

            // Application Model (NlqEntity) to REST DTO (NlqEntityDto)
            CreateMap<NlqEntity, NlqEntityDto>();

            // Application Model (NlqProcessingResult) to REST DTO (NlqResponseDto)
            CreateMap<NlqProcessingResult, NlqResponseDto>()
                .ForMember(dest => dest.OriginalQuery, opt => opt.MapFrom(src => src.OriginalQuery))
                .ForMember(dest => dest.ProcessedQuery, opt => opt.MapFrom(src => src.ProcessedQuery))
                .ForMember(dest => dest.Intent, opt => opt.MapFrom(src => src.Intent))
                .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.Entities)) // Uses NlqEntity to NlqEntityDto map
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.ConfidenceScore))
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.ResponseMessage))
                .ForMember(dest => dest.AppliedAliases, opt => opt.MapFrom(src => src.AppliedAliases))
                .ForMember(dest => dest.ProviderUsed, opt => opt.MapFrom(src => src.ProviderUsed))
                .ForMember(dest => dest.FallbackApplied, opt => opt.MapFrom(src => src.FallbackApplied));
            
            // Application Model (NlqEntity) to gRPC Model (AIService.Grpc.NlqEntity)
            CreateMap<NlqEntity, AIService.Grpc.NlqEntity>();

            // Application Model (NlqProcessingResult) to gRPC Response (AIService.Grpc.NlqResponse)
            CreateMap<NlqProcessingResult, AIService.Grpc.NlqResponse>()
                .ForMember(dest => dest.OriginalQuery, opt => opt.MapFrom(src => src.OriginalQuery))
                .ForMember(dest => dest.ProcessedQuery, opt => opt.MapFrom(src => src.ProcessedQuery))
                .ForMember(dest => dest.Intent, opt => opt.MapFrom(src => src.Intent))
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.ConfidenceScore))
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.ResponseMessage))
                .ForMember(dest => dest.ProviderUsed, opt => opt.MapFrom(src => src.ProviderUsed))
                .ForMember(dest => dest.FallbackApplied, opt => opt.MapFrom(src => src.FallbackApplied))
                .AfterMap((src, dest) => {
                    if (src.Entities != null)
                    {
                        dest.Entities.AddRange(src.Entities.Select(e => MapNlqEntityToGrpc(e)).ToList());
                    }
                    if (src.AppliedAliases != null)
                    {
                        foreach(var alias in src.AppliedAliases)
                        {
                            dest.AppliedAliases.Add(alias.Key, alias.Value);
                        }
                    }
                });
        }

        // Helper if AutoMapper has trouble with RepeatedField directly for complex types in some versions/setups
        private AIService.Grpc.NlqEntity MapNlqEntityToGrpc(NlqEntity srcEntity)
        {
            return new AIService.Grpc.NlqEntity
            {
                Name = srcEntity.Name,
                Value = srcEntity.Value,
                Type = srcEntity.Type,
                Confidence = srcEntity.Confidence
            };
        }
    }
}