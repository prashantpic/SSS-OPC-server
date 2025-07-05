using AiService.Domain.Enums;

namespace AiService.Domain.Aggregates.AiModel;

/// <summary>
/// Manages the lifecycle of an AI model, including registration, deployment, and performance tracking. 
/// Acts as a consistency boundary for all model-related operations.
/// This class is the aggregate root for an AI Model.
/// </summary>
public class AiModel
{
    /// <summary>
    /// The unique identifier for the AI model.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The user-friendly name of the model.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The version of the model, preferably following semantic versioning.
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// The type of the model, defining its purpose (e.g., Predictive Maintenance).
    /// </summary>
    public ModelType ModelType { get; private set; }

    /// <summary>
    /// The ML framework used to create the model (e.g., ONNX, TensorFlow Lite).
    /// </summary>
    public string Framework { get; private set; }

    /// <summary>
    /// The current status of the model in its lifecycle.
    /// </summary>
    public ModelStatus Status { get; private set; }

    private readonly List<ModelArtifact> _artifacts = new();
    /// <summary>
    /// A collection of physical model file artifacts associated with this model.
    /// </summary>
    public IReadOnlyCollection<ModelArtifact> Artifacts => _artifacts.AsReadOnly();

    private readonly List<ModelPerformanceLog> _performanceHistory = new();
    /// <summary>
    /// A historical log of the model's performance metrics over time.
    /// </summary>
    public IReadOnlyCollection<ModelPerformanceLog> PerformanceHistory => _performanceHistory.AsReadOnly();

    // Private constructor for ORM frameworks
#pragma warning disable CS8618
    private AiModel() { }
#pragma warning restore CS8618

    private AiModel(Guid id, string name, string version, ModelType modelType, string framework)
    {
        Id = id;
        Name = name;
        Version = version;
        ModelType = modelType;
        Framework = framework;
        Status = ModelStatus.Registered;
    }

    /// <summary>
    /// Factory method to create and register a new AI model.
    /// </summary>
    /// <param name="name">The name of the model.</param>
    /// <param name="version">The model's version.</param>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="framework">The framework used to build the model.</param>
    /// <returns>A new instance of <see cref="AiModel"/> with Registered status.</returns>
    public static AiModel Register(string name, string version, ModelType modelType, string framework)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Model name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Model version cannot be empty.", nameof(version));
        if (string.IsNullOrWhiteSpace(framework)) throw new ArgumentException("Model framework cannot be empty.", nameof(framework));

        return new AiModel(Guid.NewGuid(), name, version, modelType, framework);
    }
    
    /// <summary>
    /// Adds a file artifact to the model.
    /// </summary>
    /// <param name="storagePath">The path of the file in blob storage.</param>
    /// <param name="checksum">The checksum of the file for integrity checks.</param>
    public void AddArtifact(string storagePath, string checksum)
    {
        if (_artifacts.Any(a => a.StoragePath.Equals(storagePath, StringComparison.OrdinalIgnoreCase)))
        {
            // Or handle as an update if necessary
            return; 
        }
        _artifacts.Add(new ModelArtifact(storagePath, checksum));
    }

    /// <summary>
    /// Transitions the model's status to Deployed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the model has no artifacts or is already deployed.</exception>
    public void Deploy()
    {
        if (Status != ModelStatus.Registered)
        {
            throw new InvalidOperationException("Only registered models can be deployed.");
        }
        if (!_artifacts.Any())
        {
            throw new InvalidOperationException("Cannot deploy a model with no artifacts.");
        }
        Status = ModelStatus.Deployed;
    }
    
    /// <summary>
    /// Transitions the model's status to Retired.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the model is not in a deployed state.</exception>
    public void Retire()
    {
        if (Status != ModelStatus.Deployed)
        {
            throw new InvalidOperationException("Only deployed models can be retired.");
        }
        Status = ModelStatus.Retired;
    }

    /// <summary>
    /// Logs a new performance metric for the model.
    /// </summary>
    /// <param name="timestamp">The time the performance was measured.</param>
    /// <param name="score">The performance score (e.g., accuracy, F1-score).</param>
    public void LogPerformance(DateTime timestamp, decimal score)
    {
        _performanceHistory.Add(new ModelPerformanceLog(timestamp, score));
    }

    /// <summary>
    /// Business logic to check for significant performance degradation.
    /// This is a placeholder for more complex drift detection logic.
    /// </summary>
    /// <returns>True if performance drift is detected, otherwise false.</returns>
    public bool CheckForPerformanceDrift()
    {
        if (_performanceHistory.Count < 5) return false; // Not enough data

        var recentScores = _performanceHistory.OrderByDescending(p => p.Timestamp).Take(5).Select(p => p.Score);
        var averageRecentScore = recentScores.Average();
        var baselineScore = _performanceHistory.OrderBy(p => p.Timestamp).First().Score;

        // Drift detected if recent average performance is 10% worse than baseline
        if (averageRecentScore < (baselineScore * 0.9m))
        {
            // In a real implementation, this might raise a domain event.
            return true;
        }

        return false;
    }
}