using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Opc.System.Services.AI.API.Controllers;

#region DTOs and Commands
// DTOs and Commands are defined here to satisfy dependencies without creating unlisted files.
// Ideally, these would be in their own files in the Application layer.
public record PredictionFeedbackDto(Guid PredictionId, bool WasCorrect, string? UserComment);
public record AnomalyLabelDto(Guid AnomalyId, bool IsTrueAnomaly, string? Label);

public record SubmitFeedbackCommand(Guid Id, bool IsCorrect, string? Comment) : IRequest;
public record LabelAnomalyCommand(Guid Id, bool IsTrueAnomaly, string? Label) : IRequest;
#endregion

/// <summary>
/// REST API controller for submitting user feedback on AI results.
/// </summary>
[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly ISender _mediator;

    public FeedbackController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submits feedback on a predictive maintenance prediction.
    /// </summary>
    /// <param name="feedback">The feedback data.</param>
    /// <returns>An acknowledgment response.</returns>
    [HttpPost("prediction")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitPredictionFeedback([FromBody] PredictionFeedbackDto feedback)
    {
        // The handler for this command needs to be implemented in the Application layer.
        var command = new SubmitFeedbackCommand(feedback.PredictionId, feedback.WasCorrect, feedback.UserComment);
        await _mediator.Send(command);
        return Accepted();
    }

    /// <summary>
    /// Submits a label for a detected anomaly, confirming if it was a true or false positive.
    /// </summary>
    /// <param name="label">The anomaly label data.</param>
    /// <returns>An acknowledgment response.</returns>
    [HttpPost("anomaly")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LabelAnomaly([FromBody] AnomalyLabelDto label)
    {
        // The handler for this command needs to be implemented in the Application layer.
        var command = new LabelAnomalyCommand(label.AnomalyId, label.IsTrueAnomaly, label.Label);
        await _mediator.Send(command);
        return Accepted();
    }
}