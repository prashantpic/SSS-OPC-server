using MediatR;
using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Application.Features.ConfigurationMigrations.Services;
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Events;
using Microsoft.Extensions.Logging;
using System.IO; // For MemoryStream
using System.Threading.Tasks; // For Task.Run

namespace ManagementService.Application.Features.ConfigurationMigrations.Commands.StartConfigurationMigration;

public class StartConfigurationMigrationCommandHandler : IRequestHandler<StartConfigurationMigrationCommand, Guid>
{
    private readonly IConfigurationMigrationJobRepository _migrationJobRepository;
    private readonly ConfigurationMigrationOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartConfigurationMigrationCommandHandler> _logger;

    public StartConfigurationMigrationCommandHandler(
        IConfigurationMigrationJobRepository migrationJobRepository,
        ConfigurationMigrationOrchestrator orchestrator,
        IUnitOfWork unitOfWork,
        ILogger<StartConfigurationMigrationCommandHandler> logger)
    {
        _migrationJobRepository = migrationJobRepository;
        _orchestrator = orchestrator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(StartConfigurationMigrationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling StartConfigurationMigrationCommand for file {FileName} (Format: {Format}, Size: {Size} bytes).",
            request.FileName, request.SourceFormat, request.FileContent.Length);

        // Basic validation (FluentValidation handles most, but defensive checks are good)
        if (request.FileContent == null || request.FileContent.Length == 0)
        {
            throw new DomainException("File content cannot be empty for migration.");
        }

        // --- Create Migration Job Record ---
        var job = ConfigurationMigrationJob.Create(request.FileName, request.SourceFormat);
        await _migrationJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // Save job to get ID

        _logger.LogInformation("Configuration migration job {JobId} created for file {FileName}.", job.Id, request.FileName);

        // --- Initiate Orchestration (Asynchronous Process) ---
        // The migration process (parsing, transforming, validating, saving) can be long-running.
        // It should be executed in a background thread or task to avoid blocking the API request.
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("Starting background orchestration for migration job {JobId}.", job.Id);
                using var fileStream = new MemoryStream(request.FileContent);
                // Pass CancellationToken.None or a linked token if the background task needs its own cancellation scope
                await _orchestrator.OrchestrateMigrationAsync(job.Id, fileStream, request.SourceFormat, CancellationToken.None);
                _logger.LogInformation("Background orchestration completed for migration job {JobId}.", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background orchestration for migration job {JobId} failed.", job.Id);
                // Attempt to update the job status to Failed if an unhandled exception occurs in the orchestrator
                try
                {
                    var jobToFail = await _migrationJobRepository.GetByIdAsync(job.Id, CancellationToken.None);
                    if (jobToFail != null && jobToFail.Status != ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate.ValueObjects.JobStatus.Completed && jobToFail.Status != ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate.ValueObjects.JobStatus.Failed)
                    {
                        jobToFail.Fail($"Orchestration process failed: {ex.Message}");
                        _migrationJobRepository.Update(jobToFail);
                        await _unitOfWork.SaveChangesAsync(CancellationToken.None); // Use a new UoW instance if needed or ensure scope
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update job {JobId} status to Failed after orchestration error.", job.Id);
                }
            }
        }, cancellationToken); // Pass the original cancellation token if Task.Run should respect it.

        // --- Save and Publish Event ---
        job.AddDomainEvent(new ConfigurationMigrationInitiatedEvent(job.Id, request.FileName));
        await _unitOfWork.SaveChangesAsync(cancellationToken); // This ensures the event is dispatched if SaveChanges also handles event dispatch.

        _logger.LogInformation("Configuration migration job {JobId} initiated. Orchestration running in background.", job.Id);
        return job.Id;
    }
}