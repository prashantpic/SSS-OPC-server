using IndustrialAutomation.OpcClient.Domain.Enums;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ServerConnectionConfigDto(
        string ServerId,
        OpcStandard Standard,
        string EndpointUrl,
        string? SecurityPolicyUri, // e.g., Opc.Ua.SecurityPolicies.Basic256Sha256
        string? MessageSecurityMode, // e.g., "SignAndEncrypt", "Sign", "None" (Opc.Ua.MessageSecurityMode enum as string)
        string? UserIdentityType, // e.g., "Anonymous", "UserName", "Certificate"
        string? Username,
        string? Password,
        // UA Specific certificate details - can be part of a UA-specific config extension if preferred
        string? ClientCertificateThumbprint, 
        string? ClientCertificatePath,
        string? ClientPrivateKeyPath,
        string? TrustedPeerCertificatesPath, // Path to a directory or file of trusted server certs
        bool AutoAcceptUntrustedCertificates = false // Use with extreme caution, for development only
    );
}