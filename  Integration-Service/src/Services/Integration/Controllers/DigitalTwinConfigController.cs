using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

// Assuming AuthenticationConfigDto is defined in IoTConfigController.cs or a shared DTO location
using IntegrationService.Controllers; // For AuthenticationConfigDto

namespace IntegrationService.Controllers
{
    // Placeholder for DigitalTwinConfig DTO - Ideally, this would be in IntegrationService.Configuration
    public record DigitalTwinConfigDto(
        [Required] string Id,
        [Required] string Endpoint,
        [Required] AuthenticationConfigDto Authentication, // Reusing from IoT
        int SyncFrequencySeconds = 300,
        string? DigitalTwinModelId = null,
        string? MappingRuleId = null
    );

    // Placeholder interface for the service handling configuration logic
    public interface IDigitalTwinConfigurationService
    {
        Task<IEnumerable<DigitalTwinConfigDto>> GetAllAsync();
        Task<DigitalTwinConfigDto?> GetByIdAsync(string id);
        Task<DigitalTwinConfigDto> CreateAsync(DigitalTwinConfigDto config);
        Task<DigitalTwinConfigDto?> UpdateAsync(string id, DigitalTwinConfigDto config);
        Task<bool> DeleteAsync(string id);
    }

    [ApiController]
    [Route("api/digitaltwin/config")]
    public class DigitalTwinConfigController : ControllerBase
    {
        private readonly IDigitalTwinConfigurationService _configService;
        private readonly ILogger<DigitalTwinConfigController> _logger;

        public DigitalTwinConfigController(IDigitalTwinConfigurationService configService, ILogger<DigitalTwinConfigController> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DigitalTwinConfigDto>>> GetTwinConfigs()
        {
            _logger.LogInformation("Getting all Digital Twin configurations.");
            var configs = await _configService.GetAllAsync();
            return Ok(configs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DigitalTwinConfigDto>> GetTwinConfigById(string id)
        {
            _logger.LogInformation("Getting Digital Twin configuration with ID: {TwinId}", id);
            var config = await _configService.GetByIdAsync(id);
            if (config == null)
            {
                _logger.LogWarning("Digital Twin configuration with ID: {TwinId} not found.", id);
                return NotFound();
            }
            return Ok(config);
        }

        [HttpPost]
        public async Task<ActionResult<DigitalTwinConfigDto>> CreateTwinConfig([FromBody] DigitalTwinConfigDto config)
        {
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Creating new Digital Twin configuration with ID: {TwinId}", config.Id);
            var createdConfig = await _configService.CreateAsync(config);
            return CreatedAtAction(nameof(GetTwinConfigById), new { id = createdConfig.Id }, createdConfig);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTwinConfig(string id, [FromBody] DigitalTwinConfigDto config)
        {
            if (id != config.Id)
            {
                return BadRequest("ID mismatch.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Updating Digital Twin configuration with ID: {TwinId}", id);
            var updatedConfig = await _configService.UpdateAsync(id, config);
             if (updatedConfig == null)
            {
                _logger.LogWarning("Digital Twin configuration with ID: {TwinId} not found for update.", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTwinConfig(string id)
        {
            _logger.LogInformation("Deleting Digital Twin configuration with ID: {TwinId}", id);
            var success = await _configService.DeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("Digital Twin configuration with ID: {TwinId} not found for deletion.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}