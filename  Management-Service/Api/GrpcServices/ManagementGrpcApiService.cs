using Grpc.Core;
using MediatR;
using AutoMapper;
using ManagementService.Application.Features.ClientConfigurations.Queries.GetClientConfiguration;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkConfiguration;
using ManagementService.Application.Features.ConfigurationMigrations.Commands.StartConfigurationMigration;
using ManagementService.Application.Features.ConfigurationMigrations.Queries.GetConfigurationMigrationJobStatus;
using ManagementService.Proto.Management.V1;
using ManagementService.Api.V1.DTOs; // For mapping intermediate DTOs if needed
using ManagementService.Domain.Aggregates.ClientInstanceAggregate; // For domain objects if mapping directly
using ManagementService.Application.Features.ClientInstances.Commands.RegisterClientInstance;
using ManagementService.Application.Features.ClientInstances.Commands.UpdateClientStatus;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkUpdate;
using ManagementService.Application.Features.BulkOperations.Queries.GetBulkOperationJobStatus;

namespace ManagementService.Api.GrpcServices;

public class ManagementGrpcApiService : ManagementApi.ManagementApiBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<ManagementGrpcApiService> _logger;

    public ManagementGrpcApiService(IMediator mediator, IMapper mapper, ILogger<ManagementGrpcApiService> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<RegisterClientResponse> RegisterClient(RegisterClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC RegisterClient: ClientName={ClientName}", request.ClientName);
        var command = new RegisterClientInstanceCommand(request.ClientName, request.InitialStatus, DateTimeOffset.UtcNow);
        var clientId = await _mediator.Send(command);
        return new RegisterClientResponse { ClientId = clientId.ToString() };
    }

    public override async Task<UpdateClientStatusResponse> UpdateClientStatus(UpdateClientStatusRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC UpdateClientStatus: ClientId={ClientId}, Status={Status}", request.ClientId, request.Status);
        if (!Guid.TryParse(request.ClientId, out var clientIdGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ClientId format."));
        }
        var command = new UpdateClientStatusCommand(clientIdGuid, request.Status, request.LastSeen.ToDateTimeOffset());
        await _mediator.Send(command);
        return new UpdateClientStatusResponse();
    }
    
    public override async Task<GetClientConfigurationResponse> GetClientConfiguration(GetClientConfigurationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetClientConfiguration for ClientId: {ClientId}", request.ClientId);
        if (!Guid.TryParse(request.ClientId, out var clientIdGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ClientId format."));
        }

        var query = new GetClientConfigurationQuery(clientIdGuid);
        var configDto = await _mediator.Send(query);

        if (configDto == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Configuration not found for client {request.ClientId}."));
        }
        
        // Manual mapping or AutoMapper profile needed for ClientConfigurationDto -> ClientConfigurationProto
        var protoConfig = _mapper.Map<ClientConfigurationProto>(configDto);
        return new GetClientConfigurationResponse { Configuration = protoConfig };
    }

    public override async Task<InitiateBulkOperationResponse> InitiateBulkConfiguration(InitiateBulkConfigurationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC InitiateBulkConfiguration for ConfigVersionId: {ConfigVersionId}", request.ConfigurationVersionId);
        if (!Guid.TryParse(request.ConfigurationVersionId, out var configVersionIdGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ConfigurationVersionId format."));
        }
        var clientInstanceIds = request.ClientInstanceIds.Select(idStr => 
            Guid.TryParse(idStr, out var guid) ? guid : throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid ClientInstanceId format: {idStr}"))
        ).ToList();

        var command = new InitiateBulkConfigurationCommand(clientInstanceIds, configVersionIdGuid);
        var jobId = await _mediator.Send(command);
        return new InitiateBulkOperationResponse { JobId = jobId.ToString() };
    }

    public override async Task<InitiateBulkOperationResponse> InitiateBulkUpdate(InitiateBulkUpdateRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC InitiateBulkUpdate for TargetVersion: {TargetVersion}", request.TargetVersion);
         var clientInstanceIds = request.ClientInstanceIds.Select(idStr => 
            Guid.TryParse(idStr, out var guid) ? guid : throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid ClientInstanceId format: {idStr}"))
        ).ToList();

        var command = new InitiateBulkUpdateCommand(clientInstanceIds, request.UpdatePackageUrl, request.TargetVersion);
        var jobId = await _mediator.Send(command);
        return new InitiateBulkOperationResponse { JobId = jobId.ToString() };
    }

    public override async Task<BulkOperationStatusResponse> GetBulkOperationStatus(GetBulkOperationStatusRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetBulkOperationStatus for JobId: {JobId}", request.JobId);
        if (!Guid.TryParse(request.JobId, out var jobIdGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid JobId format."));
        }
        var query = new GetBulkOperationJobStatusQuery(jobIdGuid);
        var jobStatusDto = await _mediator.Send(query);

        if (jobStatusDto == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Bulk operation job {request.JobId} not found."));
        }
        // Manual mapping or AutoMapper profile needed for BulkOperationJobDto -> BulkOperationStatusResponse
        var protoResponse = _mapper.Map<BulkOperationStatusResponse>(jobStatusDto);
        return protoResponse;
    }

    public override async Task<StartConfigurationMigrationResponse> StartConfigurationMigration(StartConfigurationMigrationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC StartConfigurationMigration for FileName: {FileName}", request.FileName);
        var command = new StartConfigurationMigrationCommand(request.FileContent.ToByteArray(), request.FileName, request.SourceFormat);
        var jobId = await _mediator.Send(command);
        return new StartConfigurationMigrationResponse { JobId = jobId.ToString() };
    }

    public override async Task<ConfigurationMigrationStatusResponse> GetConfigurationMigrationStatus(GetConfigurationMigrationStatusRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetConfigurationMigrationStatus for JobId: {JobId}", request.JobId);
        if (!Guid.TryParse(request.JobId, out var jobIdGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid JobId format."));
        }
        var query = new GetConfigurationMigrationJobStatusQuery(jobIdGuid);
        var migrationStatusDto = await _mediator.Send(query);
        
        if (migrationStatusDto == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Configuration migration job {request.JobId} not found."));
        }
        // Manual mapping or AutoMapper profile needed for ConfigurationMigrationJobDto -> ConfigurationMigrationStatusResponse
        var protoResponse = _mapper.Map<ConfigurationMigrationStatusResponse>(migrationStatusDto);
        return protoResponse;
    }
}