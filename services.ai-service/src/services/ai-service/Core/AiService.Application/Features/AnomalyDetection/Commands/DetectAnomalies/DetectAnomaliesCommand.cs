using AiService.Application.Dtos;
using MediatR;

namespace AiService.Application.Features.AnomalyDetection.Commands.DetectAnomalies;

/// <summary>
/// Represents a request to run an anomaly detection model on a set of data.
/// This is a CQRS command that processes a data stream or batch for anomaly detection.
/// </summary>
/// <param name="ModelId">The unique identifier of the anomaly detection model to use.</param>
/// <param name="RealTimeDataPoints">An enumerable collection of data points to be analyzed for anomalies.</param>
public record DetectAnomaliesCommand(Guid ModelId, IEnumerable<DataPointDto> RealTimeDataPoints) : IRequest<IEnumerable<AnomalyDto>>;