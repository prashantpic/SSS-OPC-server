using AIService.Api.Dtos.Nlq;
using AIService.Application.Nlq.Commands; // Assuming this namespace
using AIService.Application.Nlq.Models;   // Assuming this namespace
using AIService.Grpc; // For gRPC types
using AutoMapper;
using System.Linq;

namespace AIService.Api.Mappers
{
    public class NlqProfile : Profile
    {
        public NlqProfile()
        {
            // REST DTOs to Application Commands/Models
            CreateMap<NlqRequestDto, ProcessNlqCommand>()
                .ForMember(dest => dest.QueryText, opt => opt.MapFrom(src => src.Query))
                .ForMember(dest => dest.ContextData, opt => opt.MapFrom(src => src.Context));

            CreateMap<NlqProcessingResult, NlqResponseDto>()
                .ForMember(dest => dest.FormattedResponse, opt => opt.MapFrom(src => src.ResponseText));
            
            CreateMap<NlqEntityResult, NlqEntityDto>(); // Assuming NlqEntityResult in Application.Nlq.Models

            // gRPC Messages to Application Commands/Models
            CreateMap<ProcessNlqRequest, ProcessNlqCommand>();

            CreateMap<NlqProcessingResult, ProcessNlqResponse>()
                .ForMember(dest => dest.FormattedResponse, opt => opt.MapFrom(src => src.ResponseText))
                .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => src.Entities));
            
            CreateMap<NlqEntityResult, NlqEntity>(); // Mapping to gRPC NlqEntity

            // Mapping for shared DTOs used inside other DTOs/Models if they are complex types
            // CreateMap<Application.Nlq.Models.NlqEntity, Dtos.Nlq.NlqEntityDto>();
            // CreateMap<Application.Nlq.Models.NlqEntity, Grpc.NlqEntity>();
        }
    }
}