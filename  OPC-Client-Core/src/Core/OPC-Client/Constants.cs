namespace OPC.Client.Core
{
    /// <summary>
    /// Contains constant values used throughout the OPC Client Core library.
    /// Centralizes definition of constant strings, numbers, and other fixed values
    /// to improve maintainability and consistency.
    /// </summary>
    public static class Constants
    {
        // Configuration Keys
        public const string DefaultOpcOperationTimeoutConfigKey = "OpcClient:DefaultOperationTimeoutMs";
        public const string ClientInstanceIdConfigKey = "OpcClient:ClientInstanceId";
        public const string LoggingConfigurationSectionKey = "Serilog"; // Assuming Serilog config is under this section
        public const string SubscriptionBufferingEnabledConfigKey = "OpcClient:SubscriptionBuffering:Enabled";
        public const string SubscriptionBufferCapacityConfigKey = "OpcClient:SubscriptionBuffering:Capacity";
        public const string CriticalWriteAuditEnabledConfigKey = "OpcClient:Auditing:CriticalWriteEnabled";
        public const string CriticalWriteTagIdConfigKey = "OpcClient:Auditing:CriticalWriteTagId"; // This might be a list or pattern
        public const string WriteLimitingEnabledConfigKey = "OpcClient:WriteLimiting:Enabled";
        public const string WriteRateLimitPerTagConfigKey = "OpcClient:WriteLimiting:RateLimitPerTag";
        public const string WriteValueChangeThresholdConfigKey = "OpcClient:WriteLimiting:ValueChangeThreshold";
        public const string EdgeAIEnabledConfigKey = "OpcClient:EdgeAI:Enabled";

        // Default Values
        public const int DefaultOpcOperationTimeoutMs = 10000; // 10 seconds
        public const int DefaultSubscriptionBufferCapacity = 1000;

        // RabbitMQ Configuration Keys & Defaults
        public const string RabbitMqConfigSectionKey = "RabbitMq";
        public const string RabbitMqHostnameConfigKey = RabbitMqConfigSectionKey + ":Hostname";
        public const string RabbitMqPortConfigKey = RabbitMqConfigSectionKey + ":Port";
        public const string RabbitMqUsernameConfigKey = RabbitMqConfigSectionKey + ":Username";
        public const string RabbitMqPasswordConfigKey = RabbitMqConfigSectionKey + ":Password";
        public const string RabbitMqVirtualHostConfigKey = RabbitMqConfigSectionKey + ":VirtualHost";
        public const string RabbitMqUseTlsConfigKey = RabbitMqConfigSectionKey + ":UseTls";
        public const string RabbitMqDataExchangeName = "opc.client.data.exchange";
        public const string RabbitMqAlarmEventExchangeName = "opc.client.alarm.exchange";
        public const string RabbitMqAuditLogExchangeName = "opc.client.audit.exchange";
        public const string RabbitMqHealthStatusExchangeName = "opc.client.health.exchange";


        // gRPC Configuration Keys
        public const string GrpcServerEndpointConfigKey = "GrpcClient:ServerEndpoint";
        public const string GrpcUseTlsConfigKey = "GrpcClient:UseTls";

        // OPC UA Specific Configuration Keys & Defaults
        public const string OpcUaApplicationNameConfigKey = "OpcUa:ApplicationName";
        public const string OpcUaApplicationUriConfigKey = "OpcUa:ApplicationUri";
        public const string OpcUaProductUriConfigKey = "OpcUa:ProductUri";
        public const string OpcUaDefaultSessionTimeoutConfigKey = "OpcUa:DefaultSessionTimeoutMs";

        // OPC UA Security Configuration Keys
        public const string OpcUaSecurityConfigSectionKey = "OpcUa:Security";
        public const string OpcUaSecurityPolicyUriConfigKey = OpcUaSecurityConfigSectionKey + ":SecurityPolicyUri";
        public const string OpcUaMessageSecurityModeConfigKey = OpcUaSecurityConfigSectionKey + ":MessageSecurityMode";
        public const string OpcUaClientCertificateThumbprintConfigKey = OpcUaSecurityConfigSectionKey + ":ClientCertificate:Thumbprint";
        public const string OpcUaClientCertificateStorePathConfigKey = OpcUaSecurityConfigSectionKey + ":ClientCertificate:StorePath";
        public const string OpcUaClientCertificateFilePathConfigKey = OpcUaSecurityConfigSectionKey + ":ClientCertificate:FilePath";
        public const string OpcUaClientCertificateFilePasswordConfigKey = OpcUaSecurityConfigSectionKey + ":ClientCertificate:FilePassword";
        public const string OpcUaTrustedPeerCertificatesStorePathConfigKey = OpcUaSecurityConfigSectionKey + ":TrustedPeerCertificatesStorePath";
        public const string OpcUaRejectedCertificateStorePathConfigKey = OpcUaSecurityConfigSectionKey + ":RejectedCertificateStorePath";
        public const string OpcUaIssuerCertificateStorePathConfigKey = OpcUaSecurityConfigSectionKey + ":IssuerCertificateStorePath";
        public const string OpcUaAutoAcceptUntrustedCertificatesConfigKey = OpcUaSecurityConfigSectionKey + ":AutoAcceptUntrustedCertificates";

        // OPC UA User Identity Configuration Keys
        public const string OpcUaUserIdentityTypeConfigKey = "OpcUa:UserIdentity:Type";
        public const string OpcUaUserIdentityUsernameConfigKey = "OpcUa:UserIdentity:Username";
        public const string OpcUaUserIdentityPasswordConfigKey = "OpcUa:UserIdentity:Password";
        public const string OpcUaUserIdentityCertThumbprintConfigKey = "OpcUa:UserIdentity:CertificateThumbprint";
        public const string OpcUaUserIdentityCertStorePathConfigKey = "OpcUa:UserIdentity:CertificateStorePath";
        public const string OpcUaUserIdentityCertFilePathConfigKey = "OpcUa:UserIdentity:CertificateFilePath";
        public const string OpcUaUserIdentityCertFilePasswordConfigKey = "OpcUa:UserIdentity:CertificateFilePassword";
        public const string OpcUaUserIdentityIssuedTokenConfigKey = "OpcUa:UserIdentity:IssuedToken";

        // ONNX Runtime Configuration Keys
        public const string OnnxModelPathConfigKey = "EdgeAI:OnnxModelPath";
    }
}