using MediatR;
using ManagementService.Api.V1.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementService.Application.Features.ClientConfigurations.Commands.CreateOrUpdateClientConfiguration;
using ManagementService.Application.Features.ClientConfigurations.Commands.AddConfigurationVersion;
using ManagementService.Application.Features.ClientConfigurations.Commands.ActivateConfigurationVersion;
using ManagementService.Application.Features.ClientConfigurations.Queries.GetClientConfiguration;
using ManagementService.Application.Features.ConfigurationMigrations.Commands.StartConfigurationMigration;
using ManagementService.Application.Features.ConfigurationMigrations.Queries.GetConfigurationMigrationJobStatus;

namespace ManagementService.Api.Controllers;

[ApiController]
[Route("api/v1/clients/{clientId}/configurations")]
public class ClientConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientConfigurationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the configuration for a specific client.
    /// REQ-6-001
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ClientConfigurationDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetClientConfiguration(Guid clientId)
    {
        var query = new GetClientConfigurationQuery(clientId);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates or updates the configuration for a specific client.
    /// If a configuration exists, a new version is added. If not, a new configuration is created.
    /// REQ-6-001
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateOrUpdateConfiguration(Guid clientId, [FromBody] CreateOrUpdateConfigurationRequestDto request)
    {
        var command = new CreateOrUpdateClientConfigurationCommand(clientId, request.Name, request.Content);
        var result = await _mediator.Send(command); // Handler returns (configId, newVersionId, wasCreated)

        if (result.WasCreated)
        {
            return CreatedAtAction(nameof(GetClientConfiguration), new { clientId = clientId }, result.ConfigurationId);
        }
        return Ok(result.ConfigurationId);
    }

    /// <summary>
    /// Adds a new version to an existing client configuration.
    /// REQ-6-001
    /// </summary>
    [HttpPost("~/api/v1/configurations/{configurationId}/versions")] // Different route structure as per SDS
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> AddNewVersion(Guid configurationId, [FromBody] AddConfigurationVersionRequestDto request)
    {
        var command = new AddConfigurationVersionCommand(configurationId, request.Content);
        var versionId = await _mediator.Send(command);
        // Consider returning the full version DTO or link to it
        return Created($"/api/v1/configurations/{configurationId}/versions/{versionId}", versionId);
    }

    /// <summary>
    /// Activates a specific version of a client configuration.
    /// REQ-6-001
    /// </summary>
    [HttpPost("~/api/v1/configurations/{configurationId}/versions/{versionId}/activate")] // Different route structure
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ActivateVersion(Guid configurationId, Guid versionId)
    {
        var command = new ActivateConfigurationVersionCommand(configurationId, versionId);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>
    /// Initiates a configuration migration from a file.
    /// REQ-SAP-009
    /// </summary>
    [HttpPost("~/api/v1/configurations/migrate")] // Different route structure
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> StartMigration([FromBody] ConfigurationMigrationRequestDto request)
    {
        if (string.IsNullOrEmpty(request.FileContentBase64) || string.IsNullOrEmpty(request.FileName) || string.IsNullOrEmpty(request.SourceFormat))
        {
            return BadRequest("File content, name, and format are required.");
        }
        try
        {
            var fileContentBytes = Convert.FromBase64String(request.FileContentBase64);
            var command = new StartConfigurationMigrationCommand(fileContentBytes, request.FileName, request.SourceFormat);
            var jobId = await _mediator.Send(command);
            return AcceptedAtAction(nameof(GetMigrationStatus), new { jobId = jobId }, jobId);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid Base64 file content.");
        }
    }

    /// <summary>
    /// Gets the status of a configuration migration job.
    /// REQ-SAP-009
    /// </summary>
    [HttpGet("~/api/v1/migrations/{jobId}/status")] // Different route structure
    [ProducesResponseType(typeof(ConfigurationMigrationJobDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetMigrationStatus(Guid jobId)
    {
        var query = new GetConfigurationMigrationJobStatusQuery(jobId);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }
}

// Defined here as per SDS structure for this file
public record CreateOrUpdateConfigurationRequestDto(string Name, string Content);
public record AddConfigurationVersionRequestDto(string Content);