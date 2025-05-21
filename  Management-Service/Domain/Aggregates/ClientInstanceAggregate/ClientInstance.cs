using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate.ValueObjects;
using System;
using ManagementService.Domain.Events; // Assuming domain events live here

namespace ManagementService.Domain.Aggregates.ClientInstanceAggregate;

public class ClientInstance : Entity, IAggregateRoot
{
    public string Name { get; private set; }
    public ClientStatusValueObject Status { get; private set; }
    public DateTimeOffset? LastSeen { get; private set; }
    public Guid? ClientConfigurationId { get; private set; } // FK to its active configuration
    public virtual ClientConfiguration? ClientConfiguration { get; private set; } // Navigation property

    // Private constructor for EF Core and factory methods
    private ClientInstance() { }

    public static ClientInstance Create(string name, ClientStatusValueObject initialStatus, DateTimeOffset? initialLastSeen = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client instance name cannot be null or empty.");
        ArgumentNullException.ThrowIfNull(initialStatus);

        var clientInstance = new ClientInstance
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = initialStatus,
            LastSeen = initialLastSeen ?? DateTimeOffset.UtcNow
        };

        // Example of raising a domain event
        // clientInstance.AddDomainEvent(new ClientInstanceRegisteredEvent(clientInstance.Id, clientInstance.Name));
        return clientInstance;
    }

    public void UpdateStatus(ClientStatusValueObject newStatus, DateTimeOffset lastSeen)
    {
        ArgumentNullException.ThrowIfNull(newStatus);

        if (Status != newStatus || LastSeen != lastSeen)
        {
            // Example of raising a domain event
            // AddDomainEvent(new ClientStatusChangedEvent(Id, Status.Value, newStatus.Value, lastSeen));
            Status = newStatus;
            LastSeen = lastSeen;
        }
    }

    public void AssignConfiguration(ClientConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.ClientInstanceId != this.Id)
            throw new DomainException("Cannot assign configuration from a different client instance.");

        ClientConfiguration = configuration;
        ClientConfigurationId = configuration.Id;
        // Optionally set the active version of this config to the client instance
        // AddDomainEvent(new ClientConfigurationAssignedEvent(Id, configuration.Id));
    }

    public void RemoveConfiguration()
    {
        if (ClientConfigurationId.HasValue)
        {
            // AddDomainEvent(new ClientConfigurationRemovedEvent(Id, ClientConfigurationId.Value));
            ClientConfiguration = null;
            ClientConfigurationId = null;
        }
    }
}