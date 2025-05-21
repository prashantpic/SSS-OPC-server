using System;

namespace IntegrationService.Adapters.Blockchain.Models
{
    /// <summary>
    /// Model representing a record retrieved from the blockchain.
    /// </summary>
    public record BlockchainRecord
    {
        /// <summary>
        /// The hash of the blockchain transaction.
        /// </summary>
        public string TransactionHash { get; init; } = string.Empty;

        /// <summary>
        /// The block number where the transaction was included.
        /// </summary>
        public ulong BlockNumber { get; init; }

        /// <summary>
        /// The timestamp of the block or transaction.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// The data payload logged in the transaction (or its hash).
        /// </summary>
        public string DataPayload { get; init; } = string.Empty; // Matches BlockchainTransactionRequest DataPayload

        /// <summary>
        /// Optional metadata retrieved from the blockchain event or state.
        /// </summary>
        public string Metadata { get; init; } = string.Empty; // e.g., Event arguments serialized
    }
}