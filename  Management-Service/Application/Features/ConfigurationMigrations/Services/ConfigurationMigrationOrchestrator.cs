using ManagementService.Application.Abstractions.Clients;
using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Infrastructure.ConfigurationMigration.Transformers;
using ManagementService.Infrastructure.ConfigurationMigration.Validators;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using ManagementService.Domain.SeedWork;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate.ValueObjects; // For JobStatus

namespace ManagementService.Application.Features.ConfigurationMigrations.Services;

public class ConfigurationMigrationOrchestrator
{
    private readonly IConfigurationFileParserFactory _parserFactory;
    private readonly DefaultConfigurationTransformer _transformer;
    private readonly MigratedConfigurationValidator _validator;
    private readonly IClientConfigurationRepository _clientConfigurationRepository;
    private readonly IConfigurationMigrationJobRepository _migrationJobRepository;
    private readonly IClientInstanceRepository _clientInstanceRepository; // To lookup ClientInstances
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfigurationMigrationOrchestrator> _logger;

    public ConfigurationMigrationOrchestrator(
        IConfigurationFileParserFactory parserFactory,
        DefaultConfigurationTransformer transformer,
        MigratedConfigurationValidator validator,
        IClientConfigurationRepository clientConfigurationRepository,
        IConfigurationMigrationJobRepository migrationJobRepository,
        IClientInstanceRepository clientInstanceRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfigurationMigrationOrchestrator> logger)
    {
        _parserFactory = parserFactory;
        _transformer = transformer;
        _validator = validator;
        _clientConfigurationRepository = clientConfigurationRepository;
        _migrationJobRepository = migrationJobRepository;
        _clientInstanceRepository = clientInstanceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task OrchestrateMigrationAsync(Guid jobId, Stream fileStream, string sourceFormat, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting orchestration for migration job {JobId}, format: {Format}.", jobId, sourceFormat);
        var job = await _migrationJobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            _logger.LogError("Migration job {JobId} not found.", jobId);
            return;
        }

        try
        {
            // 1. Parsing
            job.StartParsing();
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} status: Parsing.", jobId);

            var parser = _parserFactory.GetParser(sourceFormat);
            if (parser == null)
            {
                job.Fail($"Unsupported source format: {sourceFormat}");
                _migrationJobRepository.Update(job);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogError("Job {JobId} failed: Unsupported format {Format}.", jobId, sourceFormat);
                return;
            }
            var parsedItems = await parser.ParseAsync(fileStream, cancellationToken);
            _logger.LogInformation("Job {JobId}: Parsed {ItemCount} items.", jobId, parsedItems.Count());

            // 2. Transformation
            job.StartTransforming();
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} status: Transforming.", jobId);

            var migratedConfigurations = _transformer.Transform(parsedItems);
            _logger.LogInformation("Job {JobId}: Transformed into {ConfigCount} potential configurations.", jobId, migratedConfigurations.Count);

            // 3. Validation
            job.StartValidating();
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} status: Validating.", jobId);

            var validationResult = await _validator.ValidateAsync(migratedConfigurations, cancellationToken);
            if (!validationResult.IsValid)
            {
                job.Fail("Validation failed.");
                job.AddValidationMessages(validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList());
                _migrationJobRepository.Update(job);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogError("Job {JobId} failed due to validation errors: {Errors}", jobId, string.Join("; ", job.ValidationMessages));
                return;
            }
            _logger.LogInformation("Job {JobId}: Validation successful.", jobId);

            // 4. Saving (Persisting to DB)
            job.StartSaving();
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} status: Saving.", jobId);

            await PersistMigratedConfigurationsAsync(migratedConfigurations, cancellationToken);
            _logger.LogInformation("Job {JobId}: Saving configurations completed.", jobId);

            // 5. Complete Job
            job.Complete("Migration completed successfully.");
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} completed successfully.", jobId);
        }
        catch (DomainException ex) // Catch specific domain/application exceptions
        {
            _logger.LogError(ex, "Domain exception during migration orchestration for job {JobId}.", jobId);
            job.Fail($"Error: {ex.Message}");
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None); // Use CancellationToken.None if original is cancelled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during migration orchestration for job {JobId}.", jobId);
            job.Fail($"An unexpected error occurred: {ex.Message}");
            _migrationJobRepository.Update(job);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task PersistMigratedConfigurationsAsync(List<MigratedClientConfiguration> migratedConfigs, CancellationToken cancellationToken)
    {
        foreach (var migratedConfig in migratedConfigs)
        {
            if (!migratedConfig.ClientInstanceId.HasValue)
            {
                // This should have been caught by validator, but defensive check
                _logger.LogError("Cannot persist migrated config '{ConfigName}' for client '{ClientName}' as ClientInstanceId is missing.", migratedConfig.ConfigurationName, migratedConfig.ClientInstanceName);
                continue;
            }

            var clientInstanceId = migratedConfig.ClientInstanceId.Value;

            var existingConfiguration = await _clientConfigurationRepository.GetByClientInstanceIdAsync(clientInstanceId, cancellationToken);

            if (existingConfiguration != null)
            {
                // Configuration for this client already exists, add new versions
                _logger.LogInformation("Existing configuration found for client {ClientId}. Merging versions for config name '{ConfigName}'.", clientInstanceId, migratedConfig.ConfigurationName);
                bool versionAdded = false;
                foreach (var version in migratedConfig.Versions)
                {
                    // Check if a version with the same content already exists to avoid duplicates (optional)
                    // if (existingConfiguration.Versions.Any(v => v.Content == version.Content)) continue;

                    existingConfiguration.AddVersion(version.Content, version.CreatedAt ?? DateTimeOffset.UtcNow);
                    versionAdded = true;
                }
                if (versionAdded)
                {
                    // Potentially set the latest migrated version as active if no active version or based on policy
                    // For now, let's assume existing active version remains unless explicitly changed.
                    // If we want the latest migrated to be active:
                    // var latestMigrated = existingConfiguration.Versions.OrderByDescending(v => v.CreatedAt).First();
                    // existingConfiguration.SetActiveVersion(latestMigrated.Id);
                    _clientConfigurationRepository.Update(existingConfiguration);
                }
            }
            else
            {
                // No existing configuration for this client, create a new one
                _logger.LogInformation("No existing configuration for client {ClientId}. Creating new configuration '{ConfigName}'.", clientInstanceId, migratedConfig.ConfigurationName);

                var firstVersion = migratedConfig.Versions.First(); // Transformer ensures there's at least one
                var newConfiguration = ClientConfiguration.Create(
                    clientInstanceId,
                    migratedConfig.ConfigurationName,
                    firstVersion.Content
                );
                // If ClientConfiguration.Create adds first version and sets it active, we're good.
                // If not, add versions manually:
                // foreach (var version in migratedConfig.Versions.Skip(1)) // Assuming Create adds the first one
                // {
                //     newConfiguration.AddVersion(version.Content, version.CreatedAt ?? DateTimeOffset.UtcNow);
                // }
                // newConfiguration.SetActiveVersion(newConfiguration.Versions.OrderByDescending(v => v.CreatedAt).First().Id);


                await _clientConfigurationRepository.AddAsync(newConfiguration, cancellationToken);
            }
        }
        // All changes are saved by the UnitOfWork when OrchestrateMigrationAsync calls SaveChangesAsync
        // after this method completes or when the transaction is committed.
    }
}