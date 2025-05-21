using MediatR;
using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Application.Abstractions.Clients;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Events;
using Microsoft.Extensions.Logging;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;

namespace ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkConfiguration;

public class InitiateBulkConfigurationCommandHandler : IRequestHandler<InitiateBulkConfigurationCommand, Guid>
{
    private readonly IBulkOperationJobRepository _bulkOperationJobRepository;
    private readonly IClientInstanceRepository _clientInstanceRepository;
    private readonly IClientConfigurationRepository _clientConfigurationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitiateBulkConfigurationCommandHandler> _logger;

    public InitiateBulkConfigurationCommandHandler(
        IBulkOperationJobRepository bulkOperationJobRepository,
        IClientInstanceRepository clientInstanceRepository,
        IClientConfigurationRepository clientConfigurationRepository,
        IUnitOfWork unitOfWork,
        ILogger<InitiateBulkConfigurationCommandHandler> logger)
    {
        _bulkOperationJobRepository = bulkOperationJobRepository;
        _clientInstanceRepository = clientInstanceRepository;
        _clientConfigurationRepository = clientConfigurationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(InitiateBulkConfigurationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling InitiateBulkConfigurationCommand for config version {VersionId} and {ClientCount} clients.",
            request.ConfigurationVersionId, request.ClientInstanceIds.Count);

        // --- Validation ---
        var configVersion = await _clientConfigurationRepository.GetVersionByIdAsync(request.ConfigurationVersionId, cancellationToken);
        if (configVersion == null)
        {
            _logger.LogWarning("Configuration version {VersionId} not found for bulk operation.", request.ConfigurationVersionId);
            throw new EntityNotFoundException($"Configuration version with ID {request.ConfigurationVersionId} not found.");
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
        _logger.LogInformation("All {ClientCount} client IDs and configuration version validated successfully.", existingClientIds.Count);

        // --- Create Bulk Operation Job ---
        var job = BulkOperationJob.CreateBulkConfigurationDeploymentJob(
            request.ClientInstanceIds,
            request.ConfigurationVersionId
        );

        foreach (var clientId in request.ClientInstanceIds)
        {
            job.AddTask(clientId, "Pending", $"Awaiting configuration deployment with version {request.ConfigurationVersionId}");
        }

        await _bulkOperationJobRepository.AddAsync(job, cancellationToken);

        // --- Save and Publish Event ---
        job.AddDomainEvent(new BulkOperationStartedEvent(job.Id, job.OperationType.Value));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bulk configuration job {JobId} created successfully.", job.Id);

        // The actual dispatch of configuration to clients is handled by a separate process
        // reacting to the BulkOperationStartedEvent or by a scheduled task picking up pending jobs.
        // This handler's responsibility is to initiate and record the job.

        return job.Id;
    }
}