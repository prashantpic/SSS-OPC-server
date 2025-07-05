using LicensingService.Application.Contracts.Persistence;
using LicensingService.Application.Exceptions;
using LicensingService.Domain.Enums;
using LicensingService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

#region Placeholder Configuration
// This type would normally be in a separate file, potentially in the Infrastructure project for DI setup.
// It is defined here to make the handler compilable.
namespace LicensingService.Application.Configuration
{
    public class LicensingOptions
    {
        public int GracePeriodHours { get; set; } = 72;
    }
}
#endregion

namespace LicensingService.Application.Features.Licenses.Queries.ValidateLicense;

/// <summary>
/// Output DTO for the validation query.
/// </summary>
/// <param name="LicenseKey">The license key that was validated.</param>
/// <param name="IsValid">Indicates if the license is currently valid for use.</param>
/// <param name="Status">The current status of the license.</param>
/// <param name="GracePeriodWarning">A warning message if the license is operating in a grace period.</param>
/// <param name="Features">A list of feature codes enabled by this license.</param>
public record LicenseValidationDto(
    string LicenseKey, 
    bool IsValid, 
    LicenseStatus Status, 
    string? GracePeriodWarning,
    string[] Features);

/// <summary>
/// Handles the logic for the ValidateLicenseQuery use case, including grace period logic.
/// Retrieves license details and determines its validity, applying grace period rules if necessary.
/// </summary>
public class ValidateLicenseQueryHandler : IRequestHandler<ValidateLicenseQuery, LicenseValidationDto>
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Configuration.LicensingOptions _licensingOptions;

    public ValidateLicenseQueryHandler(ILicenseRepository licenseRepository, IUnitOfWork unitOfWork, IOptions<Configuration.LicensingOptions> licensingOptions)
    {
        _licenseRepository = licenseRepository;
        _unitOfWork = unitOfWork;
        _licensingOptions = licensingOptions.Value;
    }

    /// <summary>
    /// Implements the logic to check a license's validity, accounting for status, expiration, and grace periods.
    /// </summary>
    /// <param name="request">The query containing the license key to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DTO containing the result of the validation.</returns>
    /// <exception cref="NotFoundException">Thrown if the license key does not exist.</exception>
    public async Task<LicenseValidationDto> Handle(ValidateLicenseQuery request, CancellationToken cancellationToken)
    {
        var licenseKey = new LicenseKey(request.LicenseKey);
        var license = await _licenseRepository.GetByKeyAsync(licenseKey, cancellationToken);
        if (license is null)
        {
            throw new NotFoundException($"License with key '{request.LicenseKey}' was not found.");
        }

        bool isValid = license.IsValid();
        string? gracePeriodWarning = null;

        if (isValid)
        {
            license.RecordValidation();
            await _licenseRepository.UpdateAsync(license, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var gracePeriodDuration = TimeSpan.FromHours(_licensingOptions.GracePeriodHours);
            if (license.IsInGracePeriod(gracePeriodDuration))
            {
                isValid = true;
                gracePeriodWarning = $"License validation failed, but operating in grace period. Expires in {gracePeriodDuration - (DateTime.UtcNow - license.LastValidatedOn!.Value):hh\\:mm}.";
            }
        }
        
        var features = license.Features.Select(f => f.FeatureCode).ToArray();

        return new LicenseValidationDto(
            license.Key.Value,
            isValid,
            license.Status,
            gracePeriodWarning,
            features
        );
    }
}