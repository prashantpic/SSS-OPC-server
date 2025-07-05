using MediatR;

namespace LicensingService.Application.Features.Licenses.Commands.ActivateLicense;

/// <summary>
/// A data-carrying object that encapsulates all necessary information to perform the license activation use case.
/// This is a command to activate a license key with associated metadata.
/// </summary>
/// <param name="LicenseKey">The public-facing license key to activate.</param>
/// <param name="ActivationMetadata">Metadata associated with the activation, e.g., Machine ID, User ID.</param>
public record ActivateLicenseCommand(
    string LicenseKey, 
    Dictionary<string, string> ActivationMetadata) : IRequest<Unit>;