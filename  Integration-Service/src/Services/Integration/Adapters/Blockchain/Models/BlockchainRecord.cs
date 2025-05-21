using System;

namespace IntegrationService.Adapters.Blockchain.Models
{
    /// <summary>
    /// Represents the structure of data once it has been logged to or queried from the blockchain.
    /// This typically mirrors the event structure or return values from smart contract functions.
    /// REQ-8-007, REQ-8-008
    /// </summary>
    public record BlockchainRecord
    {
        /// <summary>
        /// The transaction hash on the blockchain.
        /// </summary>
        public required string TransactionHash { get; init; }

        /// <summary>
        /// The block number where the transaction was included.
        /// </summary>
        public long BlockNumber { get; init; }

        /// <summary>
        /// The timestamp of the original event (as logged in the transaction).
        /// </summary>
        public DateTimeOffset EventTimestamp { get; init; }

        /// <summary>
        /// The timestamp of the block where this transaction was mined.
        /// </summary>
        public DateTimeOffset BlockTimestamp { get; init; }

        /// <summary>
        /// The hash of the data that was logged.
        /// </summary>
        public required string DataHash { get; init; }

        /// <summary>
        /// Identifier of the source system or data origin.
        /// </summary>
        public required string SourceId { get; init; }

        /// <summary>
        /// Indicates if the record is considered valid (e.g., if retrieved from a query).
        /// </summary>
        public bool IsValid { get; init; } = true;
    }
}