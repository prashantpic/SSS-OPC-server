using ManagementService.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using ManagementService.Domain.Events;

namespace ManagementService.Domain.Aggregates.ClientInstanceAggregate;

public class ClientConfiguration : Entity, IAggregateRoot // Making it an aggregate root
{
    public Guid ClientInstanceId { get; private set; } // FK to ClientInstance
    public string Name { get; private set; }

    private readonly List<ConfigurationVersion> _versions = new();
    public IReadOnlyCollection<ConfigurationVersion> Versions => _versions.AsReadOnly();

    public Guid? ActiveVersionId { get; private set; }
    public ConfigurationVersion? ActiveVersion => _versions.FirstOrDefault(v => v.Id == ActiveVersionId);

    // Private constructor for EF Core and factory methods
    private ClientConfiguration() { }

    public static ClientConfiguration Create(Guid clientInstanceId, string name, string initialVersionContent)
    {
        if (clientInstanceId == Guid.Empty)
            throw new DomainException("ClientInstanceId cannot be empty for a configuration.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Configuration name cannot be null or empty.");

        var configuration = new ClientConfiguration
        {
            Id = Guid.NewGuid(),
            ClientInstanceId = clientInstanceId,
            Name = name
        };

        var initialVersion = configuration.AddVersion(initialVersionContent);
        configuration.SetActiveVersion(initialVersion.Id); // Activate the initial version

        // configuration.AddDomainEvent(new ClientConfigurationCreatedEvent(configuration.Id, configuration.Name, clientInstanceId));
        return configuration;
    }

    public ConfigurationVersion AddVersion(string content, DateTimeOffset? createdAt = null)
    {
        // Content validation (e.g., not null, valid JSON/XML) could happen here or via a service
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Configuration version content cannot be empty.");

        var newVersionNumber = (_versions.Any() ? _versions.Max(v => v.VersionNumber) : 0) + 1;
        var version = ConfigurationVersion.Create(newVersionNumber, content, createdAt ?? DateTimeOffset.UtcNow);
        _versions.Add(version);

        // AddDomainEvent(new ConfigurationVersionAddedEvent(Id, version.Id, version.VersionNumber));
        return version;
    }

    public void SetActiveVersion(Guid versionId)
    {
        var versionToActivate = _versions.FirstOrDefault(v => v.Id == versionId);
        if (versionToActivate == null)
            throw new DomainException($"Version with ID {versionId} not found in this configuration.");

        if (ActiveVersionId != versionId)
        {
            ActiveVersionId = versionId;
            // AddDomainEvent(new ActiveConfigurationVersionSetEvent(Id, versionId));
        }
    }
}