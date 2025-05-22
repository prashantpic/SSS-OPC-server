using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces;

/// <summary>
/// Defines the contract for managing the OPC client service's configurations, 
/// including initial loading and dynamic updates.
/// </summary>
public interface IConfigurationManagementService
{
    Task LoadInitialConfigurationAsync();

    Task ApplyServerConfigurationAsync(ClientConfigurationDto config);

    Task<TagDefinitionDto?> GetTagDefinitionAsync(string tagId);

    Task<IEnumerable<TagDefinitionDto>> GetAllTagDefinitionsAsync();

    ClientConfigurationDto GetCurrentClientConfiguration();
}