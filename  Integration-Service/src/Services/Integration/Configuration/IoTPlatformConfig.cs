namespace IntegrationService.Configuration
{
    public enum IoTPlatformType
    {
        MQTT,
        AMQP,
        HTTP
    }

    public class AuthenticationConfig
    {
        public string Type { get; set; } = "None"; // e.g., "ApiKey", "OAuth2", "UsernamePassword", "Certificate", "None"
        public string? CredentialIdentifier { get; set; }
        // Example for Username/Password or API Key
        public string? Username { get; set; }
        public string? PasswordSecretIdentifier { get; set; } // Reference to ICredentialManager
        public string? ApiKeySecretIdentifier { get; set; } // Reference to ICredentialManager
        // Example for Certificate
        public string? ClientCertificatePath { get; set; }
        public string? ClientCertificatePasswordSecretIdentifier { get; set; }
    }

    public class IoTPlatformConfig
    {
        public string Id { get; set; } = string.Empty;
        public IoTPlatformType Type { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public AuthenticationConfig Authentication { get; set; } = new AuthenticationConfig();
        public string DataFormat { get; set; } = "JSON"; // e.g., JSON, Protobuf
        public string? MappingRuleId { get; set; }
        public string? MappingRulePath { get; set; }
        public bool IsEnabled { get; set; } = true;

        // MQTT specific
        public string? MqttClientId { get; set; }
        public string? DefaultTelemetryTopic { get; set; }
        public string? CommandTopic { get; set; }
        public int MqttQoSLevel { get; set; } = 1; // 0, 1, or 2

        // AMQP specific
        public string? AmqpEntityPath { get; set; } // e.g., queue name or topic name

        // HTTP specific
        public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
    }
}