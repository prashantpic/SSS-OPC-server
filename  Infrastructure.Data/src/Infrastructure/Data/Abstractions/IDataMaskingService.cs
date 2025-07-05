using Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels;
// using Opc.System.Domain.Models; // Assuming User is in a domain project
using System.Collections.Generic;

// Placeholder for Domain Model
public class User {
    public string? Email { get; set; }
    public string? Username { get; set; }
}

namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// Defines a contract for services that mask or anonymize sensitive data.
    /// </summary>
    public interface IDataMaskingService
    {
        /// <summary>
        /// Creates a new User object with sensitive fields replaced with non-sensitive placeholders.
        /// </summary>
        /// <param name="user">The original user object.</param>
        /// <returns>A new user object with masked data.</returns>
        User MaskUserData(User user);

        /// <summary>
        /// Anonymizes a stream of historical data by applying transformations like jitter to numerical values.
        /// </summary>
        /// <param name="data">The original collection of historical data points.</param>
        /// <returns>An enumerable of new data points with anonymized values.</returns>
        IEnumerable<HistoricalDataPoint> AnonymizeHistoricalData(IEnumerable<HistoricalDataPoint> data);
    }
}