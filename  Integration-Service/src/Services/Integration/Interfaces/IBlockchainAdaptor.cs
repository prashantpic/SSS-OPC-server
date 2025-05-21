using IntegrationService.Adapters.Blockchain.Models; // Assuming DTOs will be in this namespace
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    public interface IBlockchainAdaptor
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task<BlockchainRecord?> LogCriticalDataAsync(BlockchainTransactionRequest request, CancellationToken cancellationToken = default);
        Task<BlockchainRecord?> QueryTransactionAsync(string transactionHash, CancellationToken cancellationToken = default);
        bool IsConnected { get; }
    }
}