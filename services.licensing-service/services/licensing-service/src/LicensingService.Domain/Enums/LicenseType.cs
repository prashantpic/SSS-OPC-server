namespace LicensingService.Domain.Enums;

/// <summary>
/// Defines the supported license models, such as Per-User, Per-Site, or Subscription-based.
/// This provides a strongly-typed representation of the commercial models of a license.
/// </summary>
public enum LicenseType
{
    /// <summary>
    /// License is granted on a per-user basis.
    /// </summary>
    PerUser,

    /// <summary>
    /// License is granted for an entire site or location.
    /// </summary>
    PerSite,

    /// <summary>
    /// License is based on a recurring subscription.
    /// </summary>
    Subscription
}