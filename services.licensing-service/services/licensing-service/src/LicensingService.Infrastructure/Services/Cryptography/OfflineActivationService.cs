using LicensingService.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

#region Placeholder Configuration & DTOs
// These would normally live in more appropriate locations.
namespace LicensingService.Infrastructure.Configuration
{
    public class CryptographySettings
    {
        public string OfflineActivationPrivateKey { get; set; } = string.Empty; // Should be in PEM format
        public string OfflineActivationVendorPublicKey { get; set; } = string.Empty; // Should be in PEM format
    }
}

namespace LicensingService.Infrastructure.Models
{
    public record ActivationRequestPayload(string LicenseKey, string MachineId, long Timestamp);
    public record SignedFile(string Payload, string Signature);
}
#endregion


namespace LicensingService.Infrastructure.Services.Cryptography
{
    /// <summary>
    /// Handles the cryptographic operations necessary for the secure offline activation process,
    /// including signing request files and verifying response files.
    /// </summary>
    public class OfflineActivationService : IOfflineActivationService
    {
        private readonly RSA _privateKey;
        private readonly RSA _publicKey;

        public OfflineActivationService(IOptions<Configuration.CryptographySettings> cryptoSettings)
        {
            if (string.IsNullOrWhiteSpace(cryptoSettings.Value.OfflineActivationPrivateKey) || 
                string.IsNullOrWhiteSpace(cryptoSettings.Value.OfflineActivationVendorPublicKey))
            {
                throw new InvalidOperationException("Offline activation cryptographic keys are not configured.");
            }
            
            _privateKey = RSA.Create();
            _privateKey.ImportFromPem(cryptoSettings.Value.OfflineActivationPrivateKey);
            
            _publicKey = RSA.Create();
            _publicKey.ImportFromPem(cryptoSettings.Value.OfflineActivationVendorPublicKey);
        }

        /// <summary>
        /// Creates a signed, tamper-proof file content for an offline activation request.
        /// </summary>
        /// <param name="licenseKey">The license key being activated.</param>
        /// <param name="machineId">The unique identifier of the machine requesting activation.</param>
        /// <returns>A Base64 encoded string representing the signed file.</returns>
        public string GenerateSignedActivationFile(string licenseKey, string machineId)
        {
            var payloadObject = new Models.ActivationRequestPayload(licenseKey, machineId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            string payloadJson = JsonSerializer.Serialize(payloadObject);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

            byte[] signature = _privateKey.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var signedFile = new Models.SignedFile(
                Convert.ToBase64String(payloadBytes), 
                Convert.ToBase64String(signature)
            );

            string signedFileJson = JsonSerializer.Serialize(signedFile);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(signedFileJson));
        }

        /// <summary>
        /// Verifies a signed response from the vendor and extracts the activation data.
        /// For this service, it is assumed the response is also a signed file containing the original payload.
        /// In a real scenario, the response payload might be different (e.g., containing an activation token).
        /// </summary>
        /// <param name="signedVendorResponse">The Base64 encoded signed response file.</param>
        /// <returns>The extracted license key and activation metadata.</returns>
        /// <exception cref="CryptographicException">Thrown if the signature is invalid.</exception>
        public (string LicenseKey, Dictionary<string, string> Metadata) VerifyAndExtractActivationData(string signedVendorResponse)
        {
            byte[] signedFileBytes = Convert.FromBase64String(signedVendorResponse);
            string signedFileJson = Encoding.UTF8.GetString(signedFileBytes);
            
            var signedFile = JsonSerializer.Deserialize<Models.SignedFile>(signedFileJson);
            if(signedFile is null) throw new CryptographicException("Invalid signed response file format.");

            byte[] payloadBytes = Convert.FromBase64String(signedFile.Payload);
            byte[] signature = Convert.FromBase64String(signedFile.Signature);
            
            // In this example, we assume the vendor signs the response with THEIR private key,
            // and we verify with their PUBLIC key.
            bool isValid = _publicKey.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (!isValid)
            {
                throw new CryptographicException("Signature verification failed. The response file may be tampered with or invalid.");
            }
            
            var payloadObject = JsonSerializer.Deserialize<Models.ActivationRequestPayload>(Encoding.UTF8.GetString(payloadBytes));
            if (payloadObject is null) throw new CryptographicException("Could not deserialize activation payload.");

            var metadata = new Dictionary<string, string>
            {
                { "MachineId", payloadObject.MachineId },
                { "ActivationMethod", "Offline" }
            };

            return (payloadObject.LicenseKey, metadata);
        }
    }
}