using LicensingService.Application.Contracts.Persistence;
using LicensingService.Application.Exceptions;
using LicensingService.Domain.ValueObjects;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService.Application.Features.Licenses.Commands.ActivateLicense;

/// <summary>
/// Handles the logic for the ActivateLicenseCommand use case.
/// Orchestrates the activation of a license by interacting with the domain model and persistence.
/// </summary>
public class ActivateLicenseCommandHandler : IRequestHandler<ActivateLicenseCommand, Unit>
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateLicenseCommandHandler"/> class.
    /// </summary>
    /// <param name="licenseRepository">The repository for license data access.</param>
    /// <param name="unitOfWork">The unit of work for saving changes.</param>
    public ActivateLicenseCommandHandler(ILicenseRepository licenseRepository, IUnitOfWork unitOfWork)
    {
        _licenseRepository = licenseRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Implements the core workflow for activating a license, including finding the license, invoking domain logic, and persisting the result.
    /// </summary>
    /// <param name="request">The command containing activation details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Unit value indicating successful completion.</returns>
    /// <exception cref="NotFoundException">Thrown when the license with the given key is not found.</exception>
    /// <exception cref="Domain.Exceptions.DomainException">Thrown when a business rule is violated during activation.</exception>
    public async Task<Unit> Handle(ActivateLicenseCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the License aggregate from the repository.
        var licenseKey = new LicenseKey(request.LicenseKey);
        var license = await _licenseRepository.GetByKeyAsync(licenseKey, cancellationToken);

        if (license is null)
        {
            throw new NotFoundException($"License with key '{request.LicenseKey}' was not found.");
        }

        // 2. Call the domain method to perform the activation logic.
        // The domain object enforces all business rules (e.g., checks status).
        license.Activate(request.ActivationMetadata);

        // 3. Persist the changes to the database.
        await _licenseRepository.UpdateAsync(license, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}