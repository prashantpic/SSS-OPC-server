using Opc.System.Infrastructure.Data.Abstractions;
using Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels;
// using Opc.System.Domain.Models;
using System.Collections.Generic;

namespace Opc.System.Infrastructure.Data.Services
{
    /// <summary>
    /// A service that implements data masking and anonymization techniques to protect sensitive information in non-production contexts.
    /// </summary>
    public class DataMaskingService : IDataMaskingService
    {
        private readonly Random _random = new();
        private const string MaskedValue = "[REDACTED]";
        
        /// <summary>
        /// Anonymizes a stream of historical data by applying transformations like jitter to numerical values.
        /// </summary>
        /// <param name="data">The original collection of historical data points.</param>
        /// <returns>An enumerable of new data points with anonymized values.</returns>
        public IEnumerable<HistoricalDataPoint> AnonymizeHistoricalData(IEnumerable<HistoricalDataPoint> data)
        {
            foreach (var point in data)
            {
                object? anonymizedValue = point.Value;

                if (point.Value is double d)
                {
                    // Apply a random jitter between -5% and +5%
                    var jitter = (_random.NextDouble() * 0.1) - 0.05;
                    anonymizedValue = d * (1 + jitter);
                }
                else if (point.Value is int i)
                {
                    var jitter = (_random.NextDouble() * 0.1) - 0.05;
                    anonymizedValue = (int)(i * (1 + jitter));
                }
                else if (point.Value is long l)
                {
                    var jitter = (_random.NextDouble() * 0.1) - 0.05;
                    anonymizedValue = (long)(l * (1 + jitter));
                }

                yield return new HistoricalDataPoint
                {
                    TagId = point.TagId,
                    Quality = point.Quality,
                    Timestamp = point.Timestamp,
                    Value = anonymizedValue
                };
            }
        }

        /// <summary>
        /// Creates a new User object with sensitive fields replaced with non-sensitive placeholders.
        /// </summary>
        /// <param name="user">The original user object.</param>
        /// <returns>A new user object with masked data.</returns>
        public User MaskUserData(User user)
        {
            // Create a new object to avoid side effects on the original entity
            return new User
            {
                Username = MaskedValue,
                Email = MaskedValue,
                // Copy other, non-sensitive properties if they exist
            };
        }
    }
}