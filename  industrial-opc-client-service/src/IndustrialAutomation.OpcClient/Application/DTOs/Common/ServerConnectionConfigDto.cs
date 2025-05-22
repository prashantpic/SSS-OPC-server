using IndustrialAutomation.OpcClient.Domain.Enums;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ServerConnectionConfigDto
    {
        public string ServerId { get; init; } = string.Empty;
        public OpcStandard Standard { get; init; } = OpcStandard.Ua;
        public string EndpointUrl { get; init; } = string.Empty;

        // UA Specific
        public string? SecurityPolicyUri { get; init; } // e.g., Opc.Ua.SecurityPolicies.Basic256Sha256
        public Opc.Ua.MessageSecurityMode MessageSecurityMode { get; init; } = Opc.Ua.MessageSecurityMode.SignAndEncrypt; // UA Enum
        public string UserIdentityType { get; init; } = "Anonymous"; // Anonymous, UserName, Certificate
        public string? Username { get; init; }
        public string? Password { get; init; } // Store securely if not using integrated security
        public string? ClientCertificateThumbprint { get; init; }
        public string? ClientCertificatePath { get; init; }
        public string? ClientPrivateKeyPath { get; init; }
        public bool AutoAcceptUntrustedCertificates { get; init; } = false; // For development ONLY

        // DA Specific (example, may vary based on SDK)
        public string? DaProgId { get; init; }
        public string? DaHost { get; init; }

        // XML-DA Specific (usually just endpoint)

        // HDA Specific (usually just endpoint, may have COM details if classic HDA)

        // A&C Specific (usually just endpoint, may have COM details if classic A&C)
    }
}