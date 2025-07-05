using AiService.Application.Dtos;
using MediatR;

namespace AiService.Application.Features.PredictiveMaintenance.Queries.GetMaintenancePrediction;

/// <summary>
/// Represents a request to execute a predictive maintenance model and get a forecast.
/// This is a CQRS query that will be handled to generate a maintenance prediction.
/// </summary>
/// <param name="ModelId">The unique identifier of the predictive maintenance model to use.</param>
/// <param name="InputData">A dictionary containing the input feature names and their corresponding values for the prediction.</param>
public record GetMaintenancePredictionQuery(Guid ModelId, Dictionary<string, float> InputData) : IRequest<PredictionResultDto>;