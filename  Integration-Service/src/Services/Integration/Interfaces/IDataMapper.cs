using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    public interface IDataMapper
    {
        Task<JsonDocument?> MapToExternalFormatAsync(object internalData, string mappingRuleId, CancellationToken cancellationToken = default);
        Task<JsonDocument?> MapToInternalFormatAsync(object externalData, string mappingRuleId, CancellationToken cancellationToken = default);
    }
}