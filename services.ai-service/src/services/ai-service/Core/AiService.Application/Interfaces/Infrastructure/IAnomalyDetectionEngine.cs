using AiService.Application.Dtos;

namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Defines the contract for a machine learning anomaly detection engine.
/// </summary>
public interface IAnomalyDetectionEngine
{
    /// <summary>
    /// Processes a sequence of data points to detect anomalies.
    /// </summary>
    /// <param name="modelStream">A stream containing the model data (e.g., ONNX file).</param>
    /// <param name="dataPoints">An enumerable collection of data points to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of detected anomalies.</returns>
    Task<IEnumerable<AnomalyDto>> DetectAsync(Stream modelStream, IEnumerable<DataPointDto> dataPoints, CancellationToken cancellationToken);
}