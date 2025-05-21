using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IntegrationService.Controllers
{
    // Placeholder for CriticalDataCriteria DTO
    public record CriticalDataCriteriaDto(
        [Required] string SourceProperty,
        [Required] string Operator, // e.g., "Equals", "GreaterThan", "RegexMatch"
        [Required] string Value,
        string? Description = null
    );

    // Placeholder for Blockchain configuration DTO
    public record BlockchainOverallConfigDto(
        string? RpcUrl,
        long? ChainId,
        string? SmartContractAddress,
        string? SmartContractAbiPath,
        string? CredentialIdentifier,
        string? GasPriceStrategy,
        List<CriticalDataCriteriaDto> CriticalDataCriteria
    );


    // Placeholder interface for the service handling blockchain configuration logic
    public interface IBlockchainConfigurationService
    {
        Task<BlockchainOverallConfigDto> GetConfigAsync();
        Task<BlockchainOverallConfigDto> UpdateConfigAsync(BlockchainOverallConfigDto config);
    }

    [ApiController]
    [Route("api/blockchain/config")]
    public class BlockchainConfigController : ControllerBase
    {
        private readonly IBlockchainConfigurationService _configService;
        private readonly ILogger<BlockchainConfigController> _logger;

        public BlockchainConfigController(IBlockchainConfigurationService configService, ILogger<BlockchainConfigController> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<BlockchainOverallConfigDto>> GetBlockchainConfiguration()
        {
            _logger.LogInformation("Getting blockchain configuration.");
            var config = await _configService.GetConfigAsync();
            return Ok(config);
        }

        [HttpPut]
        public async Task<ActionResult<BlockchainOverallConfigDto>> UpdateBlockchainConfiguration([FromBody] BlockchainOverallConfigDto config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Updating blockchain configuration.");
            var updatedConfig = await _configService.UpdateConfigAsync(config);
            return Ok(updatedConfig);
        }
    }
}