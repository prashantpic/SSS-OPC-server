using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for managing the OPC client service's configurations,
    /// including initial loading and dynamic updates.
    /// </summary>
    public interface IConfigurationManagementService
    {
        /// <summary>
        /// Loads the initial configuration for the client service.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadInitialConfigurationAsync();

        /// <summary>
        /// Applies a new configuration received from the server.
        /// </summary>
        /// <param name="config">The client configuration DTO from the server.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ApplyServerConfigurationAsync(ClientConfigurationDto config);

        /// <summary>
        /// Retrieves a specific tag definition by its ID.
        /// </summary>
        /// <param name="tagId">The unique identifier of the tag.</param>
        /// <returns>The tag definition DTO, or null if not found.</returns>
        Task<TagDefinitionDto?> GetTagDefinitionAsync(string tagId);

        /// <summary>
        /// Retrieves all currently active tag definitions.
        /// </summary>
        /// <returns>An enumerable collection of tag definition DTOs.</returns>
        Task<IEnumerable<TagDefinitionDto>> GetAllTagDefinitionsAsync();

        /// <summary>
        /// Gets the current comprehensive client configuration.
        /// </summary>
        /// <returns>The current client configuration DTO.</returns>
        Task<ClientConfigurationDto?> GetClientConfigurationAsync();

        /// <summary>
        /// Imports tag configurations from a specified source.
        /// </summary>
        /// <param name="importConfig">Configuration for the tag import process.</param>
        /// <returns>A task representing the asynchronous operation. True if successful, false otherwise.</returns>
        Task<bool> ImportTagConfigurationAsync(TagImportConfigDto importConfig);
    }
}