using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IntegrationService.Controllers
{
    // Placeholder for IoTPlatformConfig DTO -  Ideally, this would be in IntegrationService.Configuration
    public record IoTPlatformConfigDto(
        [Required] string Id,
        [Required] string Type, // e.g., "MQTT", "AMQP", "HTTP"
        [Required] string Endpoint,
        AuthenticationConfigDto? Authentication,
        string DataFormat = "JSON",
        string? MappingRuleId = null
    );

    public record AuthenticationConfigDto(
        [Required] string Type, // "SasKey", "Certificate", "ApiKey", "OAuth"
        [Required] string CredentialIdentifier
    );

    // Placeholder interface for the service handling configuration logic
    public interface IIoTPlatformConfigurationService
    {
        Task<IEnumerable<IoTPlatformConfigDto>>GetAllAsync();
        Task<IoTPlatformConfigDto?> GetByIdAsync(string id);
        Task<IoTPlatformConfigDto> CreateAsync(IoTPlatformConfigDto config);
        Task<IoTPlatformConfigDto?> UpdateAsync(string id, IoTPlatformConfigDto config);
        Task<bool> DeleteAsync(string id);
    }

    [ApiController]
    [Route("api/iot/config")]
    public class IoTConfigController : ControllerBase
    {
        private readonly IIoTPlatformConfigurationService _configService;
        private readonly ILogger<IoTConfigController> _logger;

        public IoTConfigController(IIoTPlatformConfigurationService configService, ILogger<IoTConfigController> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IoTPlatformConfigDto>>> GetPlatforms()
        {
            _logger.LogInformation("Getting all IoT platform configurations.");
            var configs = await _configService.GetAllAsync();
            return Ok(configs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IoTPlatformConfigDto>> GetPlatformById(string id)
        {
            _logger.LogInformation("Getting IoT platform configuration with ID: {PlatformId}", id);
            var config = await _configService.GetByIdAsync(id);
            if (config == null)
            {
                _logger.LogWarning("IoT platform configuration with ID: {PlatformId} not found.", id);
                return NotFound();
            }
            return Ok(config);
        }

        [HttpPost]
        public async Task<ActionResult<IoTPlatformConfigDto>> CreatePlatform([FromBody] IoTPlatformConfigDto config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Creating new IoT platform configuration with ID: {PlatformId}", config.Id);
            var createdConfig = await _configService.CreateAsync(config);
            return CreatedAtAction(nameof(GetPlatformById), new { id = createdConfig.Id }, createdConfig);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlatform(string id, [FromBody] IoTPlatformConfigDto config)
        {
            if (id != config.Id)
            {
                return BadRequest("ID mismatch.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Updating IoT platform configuration with ID: {PlatformId}", id);
            var updatedConfig = await _configService.UpdateAsync(id, config);
            if (updatedConfig == null)
            {
                _logger.LogWarning("IoT platform configuration with ID: {PlatformId} not found for update.", id);
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlatform(string id)
        {
            _logger.LogInformation("Deleting IoT platform configuration with ID: {PlatformId}", id);
            var success = await _configService.DeleteAsync(id);
            if (!success)
            {
                _logger.LogWarning("IoT platform configuration with ID: {PlatformId} not found for deletion.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}