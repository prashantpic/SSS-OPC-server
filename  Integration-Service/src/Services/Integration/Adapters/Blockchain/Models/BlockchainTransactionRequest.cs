using System;

namespace IntegrationService.Adapters.Blockchain.Models
{
    /// <summary>
    /// Model for requesting a blockchain transaction (e.g., logging critical data).
    /// </summary>
    public record BlockchainTransactionRequest
    {
        /// <summary>
        /// The data to be logged (or its hash).
        /// Hashing before logging is often preferred for privacy/size.
        /// </summary>
        public string DataPayload { get; init; } = string.Empty; // Could be hash or raw data (if small/non-sensitive)

        /// <summary>
        /// Timestamp associated with the data.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Identifier for the source of the data (e.g., OPC client ID, tag ID).
        /// </summary>
        public string SourceId { get; init; } = string.Empty;

        /// <summary>
        /// Any additional metadata relevant to the transaction.
        /// </summary>
        public string Metadata { get; init; } = string.Empty; // e.g., JSON string of key-value pairs

        /// <summary>
        /// Identifier for the specific smart contract function to call (if needed).
        /// </summary>
        public string? FunctionName { get; init; }
    }
}