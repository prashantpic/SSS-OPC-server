using AiService.Api.Dtos;
using AiService.Application.Dtos;
using AiService.Application.Features.PredictiveMaintenance.Queries.GetMaintenancePrediction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Api.Controllers;

/// <summary>
/// Exposes the predictive maintenance feature via a RESTful API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PredictionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<PredictionsController> _logger;

    public PredictionsController(ISender sender, ILogger<PredictionsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Requests a maintenance prediction from a specified model.
    /// </summary>
    /// <param name="modelId">The unique identifier of the predictive maintenance model.</param>
    /// <param name="requestDto">The input data for the model.</param>
    /// <returns>The result of the prediction.</returns>
    [HttpPost("{modelId:guid}")]
    [ProducesResponseType(typeof(PredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPrediction(Guid modelId, [FromBody] PredictionRequestDto requestDto)
    {
        try
        {
            var query = new GetMaintenancePredictionQuery(modelId, requestDto.InputData);
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for prediction request for model {ModelId}", modelId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
             _logger.LogWarning(ex, "Invalid operation for prediction request for model {ModelId}", modelId);
            return BadRequest(new { message = ex.Message });
        }
        // A global exception handler would catch other exceptions and return 500
    }
}