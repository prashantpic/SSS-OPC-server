using System;
using System.Collections.Generic;

namespace IntegrationService.Adapters.Blockchain.Models
{
    /// <summary>
    /// Defines the data structure needed to initiate a blockchain logging operation.
    /// REQ-8-007
    /// </summary>
    public record BlockchainTransactionRequest
    {
        /// <summary>
        /// The hash of the data to be logged. Typically a SHA256 hash.
        /// </summary>
        public required string DataHash { get; init; }

        /// <summary>
        /// Identifier for the source system or data origin.
        /// </summary>
        public required string SourceId { get; init; }

        /// <summary>
        /// Timestamp of the original event or data generation.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// Optional parameters for the smart contract function call, if different from standard.
        /// Key-value pairs where key is parameter name and value is the parameter value.
        /// This might be used for more complex smart contracts, but for a simple logData,
        /// DataHash, SourceId, and Timestamp are primary.
        /// </summary>
        public IReadOnlyDictionary<string, object>? SmartContractParameters { get; init; }
    }
}