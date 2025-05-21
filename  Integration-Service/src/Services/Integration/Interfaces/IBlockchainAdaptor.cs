using IntegrationService.Adapters.Blockchain.Models;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    /// <summary>
    /// Interface for Blockchain adaptors, defining contract for recording data.
    /// Abstracts the interaction with a specific blockchain network. Defines methods for connecting
    /// to the blockchain node, invoking smart contract functions for logging critical data,
    /// and potentially querying blockchain records for verification.
    /// </summary>
    public interface IBlockchainAdaptor
    {
        /// <summary>
        /// Establishes a connection to the blockchain node/network.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the blockchain node/network.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Logs critical data to the blockchain by invoking a smart contract function.
        /// </summary>
        /// <param name="request">The request containing data to be logged and other transaction parameters.</param>
        /// <returns>The transaction hash of the submitted blockchain transaction.</returns>
        Task<string> LogCriticalDataAsync(BlockchainTransactionRequest request);

        /// <summary>
        /// Queries a specific record from the blockchain, typically using a transaction hash or another unique identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the record to query (e.g., transaction hash).</param>
        /// <returns>The blockchain record if found; otherwise, null.</returns>
        Task<BlockchainRecord?> GetRecordAsync(string identifier);

        /// <summary>
        /// Gets a value indicating whether the adaptor is currently connected to the blockchain node.
        /// </summary>
        bool IsConnected { get; }
    }
}