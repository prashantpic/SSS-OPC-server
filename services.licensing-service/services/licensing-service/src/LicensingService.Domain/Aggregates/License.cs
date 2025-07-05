using LicensingService.Domain.Enums;
using LicensingService.Domain.ValueObjects;
using LicensingService.Domain.Entities;
using LicensingService.Domain.Exceptions;

namespace LicensingService.Domain.Aggregates;

/// <summary>
/// The License aggregate root. Represents a single software license and enforces its lifecycle rules.
/// Encapsulates all data and business rules related to a single license, ensuring its state is always consistent.
/// </summary>
public class License
{
    private readonly List<LicensedFeature> _features = new();

    /// <summary>Gets the unique identifier for the license.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the public-facing license key.</summary>
    public LicenseKey Key { get; private set; }

    /// <summary>Gets the current status of the license.</summary>
    public LicenseStatus Status { get; private set; }

    /// <summary>Gets the commercial model of the license.</summary>
    public LicenseType Type { get; private set; }

    /// <summary>Gets the feature tier of the license.</summary>
    public LicenseTier Tier { get; private set; }
    
    /// <summary>Gets the ID of the customer this license belongs to.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>Gets the timestamp of when the license was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets the expiration date for subscription-based licenses.</summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>Gets the timestamp of the first activation.</summary>
    public DateTime? ActivatedOn { get; private set; }

    /// <summary>Gets the timestamp of the last successful validation check.</summary>
    public DateTime? LastValidatedOn { get; private set; }

    /// <summary>Gets the metadata captured on activation (e.g., Machine ID).</summary>
    public Dictionary<string, string> ActivationMetadata { get; private set; } = new();

    /// <summary>Gets a read-only collection of features enabled by this license's tier.</summary>
    public IReadOnlyCollection<LicensedFeature> Features => _features.AsReadOnly();

    // Private constructor for EF Core
    private License() { }

    /// <summary>
    /// Factory method to create a new license in a valid initial state.
    /// </summary>
    public static License Create(Guid customerId, LicenseKey key, LicenseType type, LicenseTier tier, IEnumerable<LicensedFeature> features, DateTime? expirationDate)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(features);

        var license = new License
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Key = key,
            Type = type,
            Tier = tier,
            Status = LicenseStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpirationDate = expirationDate
        };
        
        license._features.AddRange(features.Where(f => f.RequiredTier <= tier));

        return license;
    }

    /// <summary>
    /// Activates the license.
    /// </summary>
    /// <param name="metadata">Metadata associated with the activation.</param>
    /// <exception cref="DomainException">Thrown if the license is not in a pending state.</exception>
    public void Activate(Dictionary<string, string> metadata)
    {
        if (Status != LicenseStatus.Pending)
        {
            throw new DomainException($"Cannot activate license. It is already in '{Status}' status.");
        }
        
        Status = LicenseStatus.Active;
        ActivatedOn = DateTime.UtcNow;
        LastValidatedOn = ActivatedOn;
        ActivationMetadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Revokes the license, making it permanently invalid.
    /// </summary>
    public void Revoke()
    {
        Status = LicenseStatus.Revoked;
    }

    /// <summary>
    /// Checks if the license is currently valid for use.
    /// </summary>
    /// <returns>True if active and not expired, otherwise false.</returns>
    public bool IsValid()
    {
        if (Status == LicenseStatus.Active)
        {
            if (ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow)
            {
                // This could be a separate 'Expire()' method called by a background job
                // For now, validation will just fail it.
                // Status = LicenseStatus.Expired;
                return false;
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Updates the timestamp of the last successful validation.
    /// </summary>
    public void RecordValidation()
    {
        LastValidatedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the license is in a grace period after a validation failure.
    /// </summary>
    /// <param name="gracePeriodDuration">The configured duration of the grace period.</param>
    /// <returns>True if the license is active and within the grace period window, otherwise false.</returns>
    public bool IsInGracePeriod(TimeSpan gracePeriodDuration)
    {
        if (Status != LicenseStatus.Active || !LastValidatedOn.HasValue)
        {
            return false;
        }

        return (DateTime.UtcNow - LastValidatedOn.Value) <= gracePeriodDuration;
    }

    /// <summary>
    /// Checks if a specific feature is enabled by this license's tier.
    /// </summary>
    /// <param name="featureCode">The unique code for the feature.</param>
    /// <returns>True if the feature is enabled, otherwise false.</returns>
    public bool IsFeatureEnabled(string featureCode)
    {
        return Features.Any(f => f.FeatureCode.Equals(featureCode, StringComparison.OrdinalIgnoreCase));
    }
}