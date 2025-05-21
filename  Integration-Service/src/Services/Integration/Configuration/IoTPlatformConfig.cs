using IntegrationService.Configuration.Models;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Configuration for a single IoT Platform.
    /// </summary>
    public class IoTPlatformConfig
    {
        /// <summary>
        /// Unique identifier for this IoT platform configuration.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of IoT platform protocol (e.g., MQTT, AMQP, HTTP).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The connection endpoint for the IoT platform.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Authentication details or references (e.g., to ICredentialManager).
        /// </summary>
        public AuthenticationSettings Authentication { get; set; } = new AuthenticationSettings();

        /// <summary>
        /// Data serialization format (e.g., JSON, Protobuf).
        /// </summary>
        public string DataFormat { get; set; } = string.Empty;

        /// <summary>
        /// Identifier or path for data mapping rules specific to this platform.
        /// </summary>
        public string MappingRuleId { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates if bi-directional communication (commands, setpoints) is enabled.
        /// </summary>
        public bool EnableBiDirectional { get; set; }
    }

    namespace Models
    {
        public class AuthenticationSettings
        {
            public string Type { get; set; } = string.Empty; // e.g., UsernamePassword, Certificate, ApiKey, ManagedIdentity, ServicePrincipal
            public string Username { get; set; } = string.Empty; // For UsernamePassword
            public string CredentialKey { get; set; } = string.Empty; // Key to lookup secret in ICredentialManager
            public string CertificateThumbprint { get; set; } = string.Empty; // For Certificate auth
            public string ApiKeyHeaderName { get; set; } = string.Empty; // For API Key auth
            public string ClientId { get; set; } = string.Empty; // For ServicePrincipal
            public string TenantId { get; set; } = string.Empty; // For ManagedIdentity or ServicePrincipal
        }
    }
}