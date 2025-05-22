using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
// Assuming generated gRPC messages will be in a specific namespace, e.g.
// using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc.Generated;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc
{
    public interface IServerAppGrpcClient
    {
        Task<ClientConfigurationDto?> GetConfigurationAsync(string clientId);
        Task<bool> SendClientStatusAsync(ClientHealthStatusDto status);

        // Potentially other methods based on opc_client_management.proto or data_ingestion.proto
        // if synchronous gRPC is used for these.
        // For example, if AI model updates are pushed via gRPC:
        // Task<bool> ReportEdgeAiOutputAsync(EdgeModelOutputDto output); // Example
        // Task<ModelUpdateResponse> CheckForModelUpdatesAsync(ModelUpdateRequest request); // Example
    }
}