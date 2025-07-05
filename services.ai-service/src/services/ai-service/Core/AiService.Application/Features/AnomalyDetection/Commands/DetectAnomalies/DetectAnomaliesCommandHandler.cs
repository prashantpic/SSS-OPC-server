using AiService.Application.Dtos;
using AiService.Application.Features.AnomalyDetection.Notifications;
using AiService.Application.Interfaces.Infrastructure;
using AiService.Domain.Enums;
using AiService.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiService.Application.Features.AnomalyDetection.Commands.DetectAnomalies;

/// <summary>
/// Processes data to find anomalies and notifies the system if any are detected.
/// This handler invokes the anomaly detection engine and publishes events for found anomalies.
/// </summary>
public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, IEnumerable<AnomalyDto>>
{
    private readonly IAnomalyDetectionEngine _anomalyDetectionEngine;
    private readonly IAiModelRepository _modelRepository;
    private readonly IModelArtifactStorage _artifactStorage;
    private readonly IPublisher _publisher;
    private readonly ILogger<DetectAnomaliesCommandHandler> _logger;

    public DetectAnomaliesCommandHandler(
        IAnomalyDetectionEngine anomalyDetectionEngine, 
        IAiModelRepository modelRepository, 
        IModelArtifactStorage artifactStorage, 
        IPublisher publisher, 
        ILogger<DetectAnomaliesCommandHandler> logger)
    {
        _anomalyDetectionEngine = anomalyDetectionEngine;
        _modelRepository = modelRepository;
        _artifactStorage = artifactStorage;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DetectAnomaliesCommand.
    /// </summary>
    /// <param name="request">The command containing the model ID and data points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of DTOs representing the detected anomalies.</returns>
    public async Task<IEnumerable<AnomalyDto>> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling anomaly detection command for model {ModelId}", request.ModelId);

        var model = await _modelRepository.GetByIdAsync(request.ModelId, cancellationToken);

        if (model is null)
        {
            _logger.LogWarning("Model with ID {ModelId} not found.", request.ModelId);
            throw new ArgumentException($"Model with ID {request.ModelId} not found.", nameof(request.ModelId));
        }

        if (model.Status != ModelStatus.Deployed)
        {
            _logger.LogWarning("Model {ModelId} is not deployed. Current status: {Status}", model.Id, model.Status);
            throw new InvalidOperationException($"Model '{model.Name}' is not deployed.");
        }

        if (model.ModelType != ModelType.AnomalyDetection)
        {
             _logger.LogWarning("Model {ModelId} is not an anomaly detection model. Type: {Type}", model.Id, model.ModelType);
            throw new InvalidOperationException($"Model '{model.Name}' is not an anomaly detection model.");
        }

        var artifact = model.Artifacts.FirstOrDefault();
        if (artifact is null)
        {
            _logger.LogError("Deployed model {ModelId} has no associated artifact.", model.Id);
            throw new InvalidOperationException($"Model '{model.Name}' is deployed but has no artifact.");
        }

        try
        {
            await using var modelStream = await _artifactStorage.GetModelStreamAsync(artifact.StoragePath, cancellationToken);
            
            var results = await _anomalyDetectionEngine.DetectAsync(modelStream, request.RealTimeDataPoints, cancellationToken);
            var anomalies = results.ToList();
            
            _logger.LogInformation("Anomaly detection complete for model {ModelId}. Found {AnomalyCount} anomalies.", request.ModelId, anomalies.Count(a => a.IsAnomaly));

            foreach (var anomaly in anomalies.Where(a => a.IsAnomaly))
            {
                await _publisher.Publish(new AnomalyDetectedEvent(anomaly), cancellationToken);
                _logger.LogInformation("Published AnomalyDetectedEvent for Tag: {Tag} at {Timestamp}", anomaly.Tag, anomaly.Timestamp);
            }

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during anomaly detection for model {ModelId}", request.ModelId);
            throw;
        }
    }
}