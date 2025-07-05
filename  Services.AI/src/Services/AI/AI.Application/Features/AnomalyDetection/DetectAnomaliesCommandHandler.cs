using MediatR;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;
using Opc.System.Services.AI.Domain.Aggregates;
using Opc.System.Services.AI.Domain.Interfaces;

namespace Opc.System.Services.AI.Application.Features.AnomalyDetection;

/// <summary>
/// Represents the command to run an anomaly detection analysis on a stream of data.
/// </summary>
/// <param name="ModelId">The ID of the anomaly detection model to use.</param>
/// <param name="Data">The time-series data points to analyze.</param>
public record DetectAnomaliesCommand(Guid ModelId, IEnumerable<TimeSeriesDataPoint> Data) : IRequest<List<AnomalyDto>>;

/// <summary>
/// Represents a single time-series data point for analysis.
/// </summary>
/// <param name="Timestamp">The timestamp of the data point.</param>
/// <param name="Value">The value of the data point.</param>
public record TimeSeriesDataPoint(DateTime Timestamp, double Value);

/// <summary>
/// Data Transfer Object representing a detected anomaly.
/// </summary>
/// <param name="Timestamp">The timestamp of the anomalous data point.</param>
/// <param name="Value">The value of the anomalous data point.</param>
/// <param name="Description">A description or score for the anomaly.</param>
public record AnomalyDto(DateTime Timestamp, double Value, string Description);

/// <summary>
/// Handles the command to run an anomaly detection analysis.
/// </summary>
public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, List<AnomalyDto>>
{
    private readonly IAiModelRepository _modelRepository;
    private readonly IModelRunner _modelRunner;
    // In a real application, you might also inject a message bus publisher (e.g., IEventPublisher)
    // to publish AnomalyDetectedEvent.

    public DetectAnomaliesCommandHandler(IAiModelRepository modelRepository, IModelRunner modelRunner)
    {
        _modelRepository = modelRepository;
        _modelRunner = modelRunner;
    }

    public async Task<List<AnomalyDto>> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the AI model metadata
        var model = await _modelRepository.GetByIdAsync(request.ModelId, cancellationToken);
        if (model is null)
        {
            throw new ApplicationException($"Model with ID {request.ModelId} not found.");
        }
        if (model.ModelType != ModelType.AnomalyDetection || model.DeploymentStatus != DeploymentStatus.Deployed)
        {
            throw new InvalidOperationException($"Model {request.ModelId} is not a deployed anomaly detection model.");
        }

        // 2. Validate input data (e.g., check for sufficient number of points)
        if (request.Data.Count() < 1)
        {
            return new List<AnomalyDto>(); // Not enough data to analyze
        }

        // 3. Prepare input for the model runner
        // The structure of the input depends on the specific ONNX model.
        // This example assumes it takes a list of values.
        var inputValues = request.Data.Select(p => p.Value).ToArray();
        var modelInput = new ModelInputData(new Dictionary<string, object>
        {
            { "float_input", inputValues }
        });

        // 4. Execute the model
        var modelOutput = await _modelRunner.RunPredictionAsync(model.Id, model.CurrentVersion.Tag, modelInput, cancellationToken);
        
        // 5. Process the output to create AnomalyDto objects
        // The model is expected to return a list of booleans or scores, one for each input point.
        var anomalyFlags = (bool[])modelOutput.Outputs["is_anomaly"];
        var anomalyScores = (float[])modelOutput.Outputs["anomaly_score"];

        var detectedAnomalies = new List<AnomalyDto>();
        var dataArray = request.Data.ToArray();

        for (int i = 0; i < anomalyFlags.Length; i++)
        {
            if (anomalyFlags[i])
            {
                var originalPoint = dataArray[i];
                var anomaly = new AnomalyDto(
                    originalPoint.Timestamp,
                    originalPoint.Value,
                    $"Anomaly detected with score: {anomalyScores[i]:F4}"
                );
                detectedAnomalies.Add(anomaly);
                
                // Here you would publish an AnomalyDetectedEvent to a message bus
                // await _eventPublisher.PublishAsync(new AnomalyDetectedEvent(anomaly));
            }
        }

        return detectedAnomalies;
    }
}