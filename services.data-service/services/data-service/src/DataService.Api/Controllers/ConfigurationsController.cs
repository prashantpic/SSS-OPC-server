using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace DataService.Api.Controllers;

// DTO, Command and Query defined here for clarity, will be moved to Application layer in a subsequent step.
public record ConfigurationDto(string Key, string Value, string? DataType, bool IsEncrypted);
public record GetConfigurationQuery(string Key) : IRequest<ConfigurationDto>;
public record SaveConfigurationCommand(string Key, string Value, string? DataType, bool IsEncrypted) : IRequest;


/// <summary>
/// API controller for managing system and user configurations.
/// Fulfills requirement REQ-DLP-008.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationsController : ControllerBase
{
    private readonly ISender _mediator;

    public ConfigurationsController(ISender mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Retrieves a configuration value by its key.
    /// </summary>
    /// <param name="key">The unique key of the configuration.</param>
    /// <returns>The configuration details.</returns>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(ConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguration(string key)
    {
        var query = new GetConfigurationQuery(key);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates or updates a configuration setting.
    /// </summary>
    /// <param name="command">The command containing the configuration key and value.</param>
    /// <returns>An HTTP 204 No Content response on success.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveConfiguration([FromBody] SaveConfigurationCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}