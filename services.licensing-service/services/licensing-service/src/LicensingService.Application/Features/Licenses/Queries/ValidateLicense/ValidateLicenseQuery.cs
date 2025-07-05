using MediatR;

namespace LicensingService.Application.Features.Licenses.Queries.ValidateLicense;

/// <summary>
/// A data-carrying object that encapsulates the information needed to perform the license validation use case.
/// Represents the query to validate a license and get its status and entitlements.
/// </summary>
/// <param name="LicenseKey">The license key to be validated.</param>
public record ValidateLicenseQuery(string LicenseKey) : IRequest<LicenseValidationDto>;