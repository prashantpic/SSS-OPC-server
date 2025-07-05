namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for managing the model lifecycle through an external MLOps platform (e.g., MLflow, Azure ML).
/// This abstracts the communication with external MLOps platforms for model tracking and lifecycle management.
/// </summary>
public interface IMlopsPlatformClient
{
    /// <summary>
    /// Logs performance metrics for a specific model version to the MLOps platform.
    /// </summary>
    /// <param name="modelName">The name of the model.</param>
    /// <param name="modelVersion">The version of the model.</param>
    /// <param name="metrics">A dictionary of metric names and their values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LogModelPerformanceAsync(string modelName, string modelVersion, Dictionary<string, double> metrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers a retraining pipeline on the MLOps platform for a given model.
    /// </summary>
    /// <param name="modelName">The name of the model to retrain.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier for the triggered pipeline run or job.</returns>
    Task<string> TriggerRetrainingPipelineAsync(string modelName, CancellationToken cancellationToken = default);
}