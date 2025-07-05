using System;
using Opc.System.Services.Management.Domain.Shared;

namespace Opc.System.Services.Management.Domain.Aggregates
{
    /// <summary>
    /// Represents a single managed OPC Client instance. It is the consistency boundary for all operations on a client.
    /// This class is the core domain model for an OPC client instance. It acts as a transactional boundary for all configuration and status updates for a specific client.
    /// </summary>
    public class OpcClientInstance
    {
        /// <summary>
        /// Unique identifier for the client instance.
        /// </summary>
        public ClientInstanceId Id { get; private set; }

        /// <summary>
        /// A user-friendly name for the instance.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The complete configuration object for the client.
        /// </summary>
        public ClientConfiguration Configuration { get; private set; }

        /// <summary>
        /// Timestamp of the last heartbeat received.
        /// </summary>
        public DateTimeOffset LastSeen { get; private set; }

        /// <summary>
        /// An enum representing the last reported health.
        /// </summary>
        public HealthStatus HealthStatus { get; private set; }

        /// <summary>
        /// A flag to enable or disable the client instance.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private OpcClientInstance() { }

        private OpcClientInstance(ClientInstanceId id, string name, ClientConfiguration configuration)
        {
            Id = id;
            Name = name;
            Configuration = configuration;
            IsActive = true;
            HealthStatus = HealthStatus.Unknown;
            LastSeen = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new OpcClientInstance.
        /// </summary>
        /// <param name="name">The user-friendly name of the instance.</param>
        /// <param name="initialConfiguration">The initial configuration for the client.</param>
        /// <returns>A new instance of OpcClientInstance.</returns>
        public static OpcClientInstance Create(string name, ClientConfiguration initialConfiguration)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Client instance name cannot be empty.", nameof(name));
            }
            ArgumentNullException.ThrowIfNull(initialConfiguration);

            var id = new ClientInstanceId(Guid.NewGuid());
            return new OpcClientInstance(id, name, initialConfiguration);
        }

        /// <summary>
        /// Validates and applies a new configuration. Raises a ClientConfigurationUpdated domain event (event logic omitted for brevity).
        /// </summary>
        /// <param name="newConfiguration">The new configuration to apply.</param>
        public void UpdateConfiguration(ClientConfiguration newConfiguration)
        {
            ArgumentNullException.ThrowIfNull(newConfiguration);
            Configuration = newConfiguration;
            // In a full implementation, a domain event would be raised here.
            // AddDomainEvent(new ClientConfigurationUpdated(this.Id));
        }

        /// <summary>
        /// Updates LastSeen and HealthStatus based on a heartbeat report.
        /// </summary>
        /// <param name="status">The current health status reported by the client.</param>
        public void ReportHeartbeat(HealthStatus status)
        {
            LastSeen = DateTimeOffset.UtcNow;
            HealthStatus = status;
        }

        /// <summary>
        /// Deactivates the client instance, stopping it from being managed.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }

    /// <summary>
    /// Represents the health status of an OPC Client instance.
    /// </summary>
    public enum HealthStatus
    {
        Unknown,
        Healthy,
        Degraded,
        Unhealthy
    }

    /// <summary>
    /// Strongly-typed identifier for OpcClientInstance.
    /// </summary>
    /// <param name="Value">The underlying Guid value.</param>
    public readonly record struct ClientInstanceId(Guid Value);
}