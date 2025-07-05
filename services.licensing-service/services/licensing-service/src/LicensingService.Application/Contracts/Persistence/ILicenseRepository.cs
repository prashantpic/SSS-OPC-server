using LicensingService.Domain.Aggregates;
using LicensingService.Domain.ValueObjects;

namespace LicensingService.Application.Contracts.Persistence;

/// <summary>
/// Defines the contract for data access operations related to the License aggregate.
/// This abstracts the persistence mechanism from the application and domain layers.
/// </summary>
public interface ILicenseRepository
{
    /// <summary>
    /// Retrieves a license aggregate by its unique license key.
    /// </summary>
    /// <param name="licenseKey">The license key value object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The license aggregate if found; otherwise, null.</returns>
    Task<License?> GetByKeyAsync(LicenseKey licenseKey, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new license aggregate to the persistence store.
    /// </summary>
    /// <param name="license">The license aggregate to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(License license, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing license aggregate in the persistence store.
    /// </summary>
    /// <param name="license">The license aggregate to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(License license, CancellationToken cancellationToken);
}