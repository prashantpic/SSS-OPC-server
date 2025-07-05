using MediatR;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.AI.Application.Features.AnomalyDetection;
using Opc.System.Services.AI.Application.Features.PredictiveMaintenance;

namespace Opc.System.Services.AI.API.Controllers;

#region Request DTOs
// DTOs are defined here to satisfy dependencies without creating unlisted files.
public record PredictionRequestDto(Guid AssetId, Guid ModelId);
public record AnomalyDetectionRequestDto(Guid ModelId, IEnumerable<TimeSeriesDataPointDto> Data);
public record TimeSeriesDataPointDto(DateTime Timestamp, double Value);
#endregion

/// <summary>
/// REST API controller for running AI analysis like predictive maintenance and anomaly detection.
/// </summary>
[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly ISender _mediator;

    public AnalysisController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Runs a predictive maintenance model for a given asset.
    /// </summary>
    /// <param name="request">The request containing the asset and model identifiers.</param>
    /// <returns>The prediction result.</returns>
    [HttpPost("predict-maintenance")]
    [ProducesResponseType(typeof(PredictionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMaintenancePrediction([FromBody] PredictionRequestDto request)
    {
        var command = new RunPredictionCommand(request.AssetId, request.ModelId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Runs an anomaly detection model on a given dataset.
    /// </summary>
    /// <param name="request">The request containing the model identifier and the data to analyze.</param>
    /// <returns>A list of detected anomalies.</returns>
    [HttpPost("detect-anomalies")]
    [ProducesResponseType(typeof(List<AnomalyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DetectAnomalies([FromBody] AnomalyDetectionRequestDto request)
    {
        var dataPoints = request.Data.Select(d => new TimeSeriesDataPoint(d.Timestamp, d.Value));
        var command = new DetectAnomaliesCommand(request.ModelId, dataPoints);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}