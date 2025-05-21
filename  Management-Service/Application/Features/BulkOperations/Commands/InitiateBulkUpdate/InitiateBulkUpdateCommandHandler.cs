using MediatR;
using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Application.Abstractions.Clients;
using ManagementService.Infrastructure.Services; // For IDeploymentOrchestrationClient
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;

namespace ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkUpdate;

public class InitiateBulkUpdateCommandHandler : IRequestHandler<InitiateBulkUpdateCommand, Guid>
{
    private readonly IBulkOperationJobRepository _bulkOperationJobRepository;
    private readonly IClientInstanceRepository _clientInstanceRepository;
    private readonly IDeploymentOrchestrationClient _deploymentOrchestrationClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitiateBulkUpdateCommandHandler> _logger;

    public InitiateBulkUpdateCommandHandler(
        IBulkOperationJobRepository bulkOperationJobRepository,
        IClientInstanceRepository clientInstanceRepository,
        IDeploymentOrchestrationClient deploymentOrchestrationClient,
        IUnitOfWork unitOfWork,
        ILogger<InitiateBulkUpdateCommandHandler> logger)
    {
        _bulkOperationJobRepository = bulkOperationJobRepository;
        _clientInstanceRepository = clientInstanceRepository;
        _deploymentOrchestrationClient = deploymentOrchestrationClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(InitiateBulkUpdateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling InitiateBulkUpdateCommand for update to version {TargetVersion} from {UpdateUrl} for {ClientCount} clients.",
            request.TargetVersion, request.UpdatePackageUrl, request.ClientInstanceIds.Count);

        // --- Validation ---
        if (string.IsNullOrWhiteSpace(request.UpdatePackageUrl))
        {
            throw new DomainException("Update package URL cannot be empty.");
        }
        if (string.IsNullOrWhiteSpace(request.TargetVersion))
        {
            throw new DomainException("Target version cannot be empty.");
        }

        var existingClientIds = new List<Guid>();
        foreach (var clientId in request.ClientInstanceIds)
        {
            var client = await _clientInstanceRepository.GetByIdAsync(clientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client instance with ID {ClientId} not found.", clientId);
                throw new EntityNotFoundException($"Client instance with ID {clientId} not found.");
            }
            existingClientIds.Add(client.Id);
        }
        _logger.LogInformation("All {ClientCount} client IDs validated successfully.", existingClientIds.Count);

        // --- Create Bulk Operation Job ---
        var jobParameters = new { request.ClientInstanceIds, request.UpdatePackageUrl, request.TargetVersion };
        var parametersJson = JsonSerializer.Serialize(jobParameters);

        var job = BulkOperationJob.CreateBulkSoftwareUpdateJob(parametersJson);

        foreach (var clientId in request.ClientInstanceIds)
        {
            job.AddTask(clientId, "Pending", $"Awaiting software update to version {request.TargetVersion}");
        }

        await _bulkOperationJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // Save job to get ID

        _logger.LogInformation("Bulk update job {JobId} created with parameters: {Parameters}", job.Id, parametersJson);

        // --- Interact with Deployment Orchestration System ---
        try
        {
            await _deploymentOrchestrationClient.ScheduleBulkUpdateAsync(
                 job.Id,
                 request.ClientInstanceIds,
                 request.UpdatePackageUrl,
                 request.TargetVersion,
                 cancellationToken);

            job.Start(); // Mark job as started/handed off
            job.UpdateDetails($"Bulk update request for version {request.TargetVersion} sent to deployment orchestration system.");
            _bulkOperationJobRepository.Update(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule bulk update job {JobId} with deployment orchestration system.", job.Id);
            job.Fail($"Failed to schedule with orchestration system: {ex.Message}");
            _bulkOperationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken); // Save failed status
            throw new DomainException($"Failed to schedule bulk update: {ex.Message}", ex);
        }

        // --- Save and Publish Event ---
        job.AddDomainEvent(new BulkOperationStartedEvent(job.Id, job.OperationType.Value));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk update job {JobId} successfully scheduled with orchestration system.", job.Id);
        return job.Id;
    }
}