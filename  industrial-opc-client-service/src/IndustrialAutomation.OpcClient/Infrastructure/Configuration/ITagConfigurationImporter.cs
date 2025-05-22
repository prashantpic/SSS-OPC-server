using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.Configuration
{
    public interface ITagConfigurationImporter
    {
        Task<List<TagDefinitionDto>> ImportTagsAsync(TagImportConfigDto config);
    }
}