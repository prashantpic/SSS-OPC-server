using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Opc.System.Services.AI.API.Controllers;

#region Commands and Queries
// DTOs, Commands and Queries are defined here to satisfy dependencies without creating unlisted files.
// Ideally, these would be in their own files in the Application layer.

public record UploadModelCommand(Stream FileStream, string FileName, string ModelType, string Version) : IRequest<Guid>;
public record DeployModelCommand(Guid ModelId, string Environment) : IRequest<Unit>;
public record GetModelDetailsQuery(Guid ModelId) : IRequest<ModelDetailsDto?>;
public record GetModelPerformanceQuery(Guid ModelId) : IRequest<ModelPerformanceDto?>;

public record ModelDetailsDto(Guid Id, string Name, string Version, string ModelType, string DeploymentStatus);
public record ModelPerformanceDto(Guid ModelId, double Accuracy, double Precision, double Recall);

#endregion

/// <summary>
/// REST API controller for managing the lifecycle of AI models.
/// Exposes endpoints for uploading, deploying, and monitoring AI models.
/// </summary>
[ApiController]
[Route("api/models")]
public class ModelManagementController : ControllerBase
{
    private readonly ISender _mediator;

    public ModelManagementController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Uploads a new AI model artifact.
    /// </summary>
    /// <param name="file">The model file (e.g., .onnx).</param>
    /// <param name="modelType">The type of model (e.g., 'PredictiveMaintenance').</param>
    /// <param name="version">The version tag for the model.</param>
    /// <returns>The ID of the newly created model.</returns>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadModel(IFormFile file, [FromForm] string modelType, [FromForm] string version)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        // The handler for this command needs to be implemented in the Application layer.
        var command = new UploadModelCommand(file.OpenReadStream(), file.FileName, modelType, version);
        var modelId = await _mediator.Send(command); 

        return CreatedAtAction(nameof(GetModelDetails), new { modelId = modelId }, new { id = modelId });
    }

    /// <summary>
    /// Deploys a specific version of an AI model to an environment.
    /// </summary>
    /// <param name="modelId">The unique identifier of the model.</param>
    /// <param name="environment">The target environment for deployment (e.g., 'Production').</param>
    /// <returns>A confirmation of the deployment operation.</returns>
    [HttpPost("{modelId}/deploy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeployModel(Guid modelId, [FromBody] string environment)
    {
        // The handler for this command needs to be implemented in the Application layer.
        var command = new DeployModelCommand(modelId, environment);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>
    /// Retrieves the details of a specific AI model.
    /// </summary>
    /// <param name="modelId">The unique identifier of the model.</param>
    /// <returns>The details of the model.</returns>
    [HttpGet("{modelId}")]
    [ProducesResponseType(typeof(ModelDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModelDetails(Guid modelId)
    {
        // The handler for this query needs to be implemented in the Application layer.
        var query = new GetModelDetailsQuery(modelId);
        var result = await _mediator.Send(query);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Retrieves the performance metrics for a specific AI model.
    /// </summary>
    /// <param name="modelId">The unique identifier of the model.</param>
    /// <returns>The performance metrics of the model.</returns>
    [HttpGet("{modelId}/performance")]
    [ProducesResponseType(typeof(ModelPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModelPerformance(Guid modelId)
    {
        // The handler for this query needs to be implemented in the Application layer.
        var query = new GetModelPerformanceQuery(modelId);
        var result = await _mediator.Send(query);
        return result is not null ? Ok(result) : NotFound();
    }
}