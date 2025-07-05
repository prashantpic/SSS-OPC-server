using Grpc.Core;
using ManagementService.Api.Grpc;
using ManagementService.Application.Features.ClientLifecycle.Commands;
using ManagementService.Domain.ValueObjects;
using MediatR;
using static ManagementService.Api.Grpc.ClientLifecycleService;

namespace ManagementService.Api.GrpcServices;

/// <summary>
/// Implements the gRPC service contract for client lifecycle management (registration and health reporting).
/// It provides a high-performance, strongly-typed API for OPC clients to communicate with the management service.
/// </summary>
public class ClientLifecycleGrpcService : ClientLifecycleServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientLifecycleGrpcService> _logger;

    public ClientLifecycleGrpcService(IMediator mediator, ILogger<ClientLifecycleGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Handles the registration of a new client instance.
    /// </summary>
    public override async Task<RegisterClientReply> RegisterClient(RegisterClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: Received registration request for client with name '{ClientName}' at site '{ClientSite}'", request.Name, request.Site);

        var command = new RegisterClientCommand(request.Name, request.Site);
        var result = await _mediator.Send(command);

        var reply = new RegisterClientReply
        {
            ClientId = result.Id.ToString(),
            Configuration = new ClientConfigurationMessage
            {
                PollingIntervalSeconds = result.InitialConfiguration.PollingIntervalSeconds
            }
        };
        
        reply.Configuration.TagConfigurations.AddRange(
            result.InitialConfiguration.TagConfigurations.Select(t => new TagConfigMessage { TagName = t.TagName, ScanRate = t.ScanRate })
        );

        return reply;
    }

    /// <summary>
    /// Handles incoming health reports from a client.
    /// </summary>
    public override async Task<ReportHealthReply> ReportHealth(ReportHealthRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ClientId, out var clientId))
        {
            _logger.LogWarning("gRPC: Received health report with invalid ClientId: {ClientId}", request.ClientId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ClientId must be a valid GUID."));
        }

        var healthStatus = new HealthStatus(
            request.HealthStatus.IsConnected,
            request.HealthStatus.DataThroughput,
            request.HealthStatus.CpuUsagePercent
        );

        var command = new ReportHealthCommand(clientId, healthStatus, DateTimeOffset.UtcNow);
        await _mediator.Send(command);

        return new ReportHealthReply();
    }
}