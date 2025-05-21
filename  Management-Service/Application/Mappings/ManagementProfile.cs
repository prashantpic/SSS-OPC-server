using AutoMapper;
using ManagementService.Api.V1.DTOs;
using ManagementService.Application.Abstractions.Services.Dto;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkConfiguration;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkUpdate;
using ManagementService.Application.Features.ConfigurationMigrations.Commands.StartConfigurationMigration;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate.ValueObjects;
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using System.Linq;

namespace ManagementService.Application.Mappings;

public class ManagementProfile : Profile
{
    public ManagementProfile()
    {
        // Domain Entity -> API DTOs
        CreateMap<ClientInstance, ClientInstanceDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Value))
            .ForMember(dest => dest.CurrentConfigurationVersionId, opt => opt.MapFrom(src =>
                src.ClientConfiguration != null ? src.ClientConfiguration.ActiveVersionId : null));

        CreateMap<ClientConfiguration, ClientConfigurationDto>()
            .ForMember(dest => dest.Versions, opt => opt.MapFrom(src => src.Versions.OrderBy(v => v.VersionNumber)));

        CreateMap<ConfigurationVersion, ConfigurationVersionDto>()
             // IsActive needs to be determined based on ClientConfiguration.ActiveVersionId
             // This mapping alone cannot determine it without context.
             // It's better handled when mapping ClientConfiguration to ClientConfigurationDto.
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());


        // API DTOs -> MediatR Commands
        CreateMap<BulkConfigurationRequestDto, InitiateBulkConfigurationCommand>();
        CreateMap<BulkUpdateRequestDto, InitiateBulkUpdateCommand>();
        CreateMap<ConfigurationMigrationRequestDto, StartConfigurationMigrationCommand>()
            .ForMember(dest => dest.FileContent, opt => opt.MapFrom(src => System.Convert.FromBase64String(src.FileContentBase64)));


        // Domain Job Entities -> API DTOs
        CreateMap<BulkOperationJob, BulkOperationJobDto>()
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => src.OperationType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalTasks, opt => opt.MapFrom(src => src.ProgressDetails.Count))
            .ForMember(dest => dest.CompletedTasks, opt => opt.MapFrom(src => src.ProgressDetails.Count(d => d.Status.Equals("Completed", System.StringComparison.OrdinalIgnoreCase))))
            .ForMember(dest => dest.FailedTasks, opt => opt.MapFrom(src => src.ProgressDetails.Count(d => d.Status.Equals("Failed", System.StringComparison.OrdinalIgnoreCase))));

        CreateMap<ConfigurationMigrationJob, ConfigurationMigrationJobDto>()
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));


        // DataService DTOs -> API DTOs
        CreateMap<AggregatedKpiResult, AggregatedKpisDto>();

        // Value Objects -> string (if needed, but often handled in specific entity mappings)
        CreateMap<ClientStatusValueObject, string>().ConvertUsing(src => src.Value);
        CreateMap<JobStatus, string>().ConvertUsing(src => src.Value);
        CreateMap<BulkOperationType, string>().ConvertUsing(src => src.Value);
    }
}