using LicensingService.Domain.ValueObjects;

// This placeholder is needed for the interface method signature. In a real scenario,
// this would be a proper reference to the command in the Application layer.
using LicensingService.Application.Features.Licenses.Commands.GenerateLicense;

namespace LicensingService.Application.Contracts.Infrastructure;

/// <summary>
/// Defines the contract for a service that generates new license keys.
/// This abstracts the specific algorithm for license key generation from the application logic.
/// </summary>
public interface ILicenseKeyGenerator
{
    /// <summary>
    /// Generates a new, unique license key.
    /// </summary>
    /// <param name="command">The command containing details about the license to be created, which can influence the key format.</param>
    /// <returns>A new LicenseKey value object.</returns>
    LicenseKey GenerateKey(GenerateLicenseCommand command);
}