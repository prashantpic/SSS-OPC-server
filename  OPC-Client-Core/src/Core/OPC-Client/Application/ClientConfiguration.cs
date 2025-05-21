using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Application.DTOs; // For UaSubscriptionConfigDto
using System.Collections.Generic;

namespace OPC.Client.Core.Application
{
    /// <summary>
    /// POCO class representing the overall configuration for a single OPC Client Core connection instance.
    /// </summary>
    /// <remarks>
    /// Includes server endpoint, security settings, and communication parameters for one OPC server.
    /// A list of these would be managed by a higher-level application configuration.
    /// Implements REQ-CSVC-001, REQ-SAP-003, REQ-3-001, REQ-CSVC-023.
    /// </remarks>
    public class ClientConfiguration
    {
        /// <summary>
        /// User-defined name for this connection configuration.
        /// </summary>
        public string Name { get; set; } = "Default OPC Connection";

        /// <summary>
        /// The endpoint URL or IP address of the OPC server.
        /// </summary>
        public string ServerEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// The type of OPC protocol to use (e.g., UA, DA).
        /// </summary>
        public OpcProtocolType ProtocolType { get; set; } = OpcProtocolType.UA;

        /// <summary>
        /// Configuration specific to OPC UA connections.
        /// Null if ProtocolType is not UA.
        /// </summary>
        public UaClientConnectionConfig? UaConfig { get; set; }

        /// <summary>
        /// Configuration specific to OPC DA connections.
        /// Null if ProtocolType is not DA.
        /// </summary>
        public DaClientConnectionConfig? DaConfig { get; set; }

        // Add configurations for other protocols (XML-DA, HDA, A&C) as needed
        // public XmlDaClientConnectionConfig? XmlDaConfig { get; set; }
        // public HdaClientConnectionConfig? HdaConfig { get; set; }
        // public AcClientConnectionConfig? AcConfig { get; set; }


        /// <summary>
        /// Default timeout for synchronous OPC operations (in milliseconds) for this connection.
        /// </summary>
        public int? DefaultOperationTimeoutMs { get; set; } = 10000; // Default 10 seconds

        /// <summary>
        /// List of tag configurations to be automatically handled by this connection (e.g., for initial subscriptions or monitoring).
        /// </summary>
        public List<TagImportConfig>? TagConfigurationsToImport { get; set; } // Path or direct configs

        /// <summary>
        /// List of OPC UA subscription configurations to be created on startup for this connection.
        /// Only applicable if ProtocolType is UA.
        /// </summary>
        public List<UaSubscriptionConfigDto>? InitialUaSubscriptions { get; set; }

        /// <summary>
        /// Configuration for client-side data validation rules specific to this connection.
        /// </summary>
        public ClientDataValidationConfig? DataValidationConfig { get; set; }

        /// <summary>
        /// Configuration for critical write auditing specific to this connection.
        /// </summary>
        public ClientCriticalWriteAuditConfig? CriticalWriteAuditConfig { get; set; }

        /// <summary>
        /// Configuration for write operation limiting specific to this connection.
        /// </summary>
        public ClientWriteOperationLimitConfig? WriteOperationLimitConfig { get; set; }

        /// <summary>
        /// Configuration for edge AI processing related to this connection's data.
        /// </summary>
        public ClientEdgeAiConfig? EdgeAiConfig { get; set; }


        // Nested configuration classes specific to this ClientConfiguration context

        /// <summary>
        /// Configuration specific to an OPC UA client connection.
        /// </summary>
        public class UaClientConnectionConfig
        {
            public UaSecurityConfiguration? SecurityConfig { get; set; }
            public UaUserIdentity? UserIdentity { get; set; }
            public uint? SessionTimeoutMs { get; set; } = 60000; // OPC UA SDK default
            public string ApplicationName { get; set; } = "OPC.Client.Core.NetCore";
            public string ApplicationUri { get; set; } = $"urn:localhost:OPC.Client.Core:{System.Guid.NewGuid()}"; // Unique per instance by default
            public string ProductUri { get; set; } = "https://opcclientcore.example.com/product";
        }

        /// <summary>
        /// Configuration specific to an OPC DA client connection.
        /// </summary>
        public class DaClientConnectionConfig
        {
            public string? ServerProgId { get; set; }
            public string? ServerClassId { get; set; }
            public string? ServerHost { get; set; } // For DCOM, usually the machine name or IP
        }

        /// <summary>
        /// Represents configuration for importing tags.
        /// </summary>
        public class TagImportConfig
        {
            public string FilePath { get; set; } = string.Empty;
            public string Format { get; set; } = "csv"; // e.g., "csv", "xml"
        }

        /// <summary>
        /// Configuration for client-side data validation for this connection.
        /// </summary>
        public class ClientDataValidationConfig
        {
            public bool EnableValidation { get; set; } = true;
            public Dictionary<string, string>? TagValidationRules { get; set; } // Key: Tag Address (string), Value: Rule (string)
        }

        /// <summary>
        /// Configuration for critical write auditing for this connection.
        /// </summary>
        public class ClientCriticalWriteAuditConfig
        {
            public bool EnableAuditing { get; set; } = true;
            public List<string>? CriticalTagNodeIds { get; set; } // List of NodeIds considered critical
        }

        /// <summary>
        /// Configuration for write operation limiting for this connection.
        /// </summary>
        public class ClientWriteOperationLimitConfig
        {
            public bool EnableLimiting { get; set; } = true;
            public int? MaxWritesPerTagPerSecond { get; set; } = 1; // Default: 1 write per tag per second
            public Dictionary<string, double>? ValueChangeThresholdsPercent { get; set; } // Key: Tag NodeId, Value: % change threshold
        }

        /// <summary>
        /// Configuration for edge AI processing related to this connection.
        /// </summary>
        public class ClientEdgeAiConfig
        {
            public bool EnableAiProcessing { get; set; } = false;
            public string? ModelPath { get; set; } // Path to the ONNX model file
            public List<string>? InputTagNodeIds { get; set; } // Tags whose data feeds the model
            public List<string>? OutputTagNodeIds { get; set; } // Tags where model results might be written (optional)
            public int? InferenceIntervalMs { get; set; } = 1000; // Default: run inference every second
        }
    }

    /// <summary>
    /// Global configuration for the entire OPC Client Core service instance,
    /// potentially managing multiple ClientConfiguration (connections).
    /// </summary>
    public class OpcClientCoreServiceConfiguration
    {
        public List<ClientConfiguration> Connections { get; set; } = new List<ClientConfiguration>();
        public ServerCommunicationConfig BackendCommunication { get; set; } = new ServerCommunicationConfig();
        public GlobalSecurityConfig GlobalSecurity { get; set; } = new GlobalSecurityConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
        public SubscriptionBufferingConfig SubscriptionBuffering { get; set; } = new SubscriptionBufferingConfig();
    }

    public class ServerCommunicationConfig
    {
        public string GrpcServerEndpoint { get; set; } = "http://localhost:50051"; // Example
        public RabbitMqConfig? RabbitMq { get; set; }
    }

    public class RabbitMqConfig
    {
        public string Hostname { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public bool UseSsl { get; set; } = false;
        public string DataExchangeName { get; set; } = "opc.client.data";
        public string AlarmEventExchangeName { get; set; } = "opc.client.alarms";
        public string AuditLogExchangeName { get; set; } = "opc.client.audit";
    }

    public class GlobalSecurityConfig
    {
        // e.g., paths to global certificate stores if not per-connection
        public string? DefaultClientCertificateStorePath { get; set; }
        public string? DefaultTrustedPeerCertificateStorePath { get; set; }
        public string? DefaultRejectedCertificateStorePath { get; set; }
        public string? DefaultIssuerCertificateStorePath { get; set; }
    }
    public class LoggingConfig
    {
        public string LogLevel { get; set; } = "Information"; // Serilog log level
        public string LogFilePath { get; set; } = "logs/opc-client-core-.log"; // Rolling file
    }

    public class SubscriptionBufferingConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxBufferSizePerSubscription { get; set; } = 1000; // Number of items
        public int MaxBufferTimeSeconds { get; set; } = 300; // Max time to buffer
    }
}