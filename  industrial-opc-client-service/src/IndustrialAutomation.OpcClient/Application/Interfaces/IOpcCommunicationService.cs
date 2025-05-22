using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda;
using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.Interfaces;

/// <summary>
/// Defines the primary contract for all OPC-related communication tasks, 
/// abstracting specific OPC protocol details.
/// </summary>
public interface IOpcCommunicationService
{
    Task ConnectAsync(ServerConnectionConfigDto config);

    Task DisconnectAsync(string serverId);

    Task<IEnumerable<UaBrowseNodeDto>> BrowseNamespaceAsync(string serverId, string nodeId);

    Task<ReadResponseDto> ReadTagsAsync(string serverId, ReadRequestDto request);

    Task<WriteResponseDto> WriteTagsAsync(string serverId, WriteRequestDto request);

    Task<string> CreateSubscriptionAsync(string serverId, UaSubscriptionConfigDto config);

    Task<bool> RemoveSubscriptionAsync(string subscriptionId);

    Task<HdaReadResponseDto> QueryHistoricalDataAsync(string serverId, HdaReadRequestDto request);

    Task<bool> AcknowledgeAlarmAsync(string serverId, AcAcknowledgeRequestDto request);
}