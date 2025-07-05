namespace Opc.System.Services.AI.Domain.Aggregates;

/// <summary>
/// Represents the AiModel aggregate root, encapsulating a machine learning model and its lifecycle.
/// It contains the state and behavior of an AI model, ensuring business rules (invariants) are maintained.
/// </summary>
public class AiModel
{
    private readonly List<ModelVersion> _versionHistory = new();
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ModelType ModelType { get; private set; }
    public DeploymentStatus DeploymentStatus { get; private set; }
    public ModelVersion CurrentVersion { get; private set; } = null!;
    public ModelPerformanceMetrics? PerformanceMetrics { get; private set; }

    public IReadOnlyCollection<ModelVersion> VersionHistory => _versionHistory.AsReadOnly();

    // Private constructor for persistence frameworks
    private AiModel() { }

    /// <summary>
    /// Factory method to create a new AI Model.
    /// </summary>
    public static AiModel Create(string name, ModelType modelType, string initialVersionTag, string checksum)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Model name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(initialVersionTag))
            throw new ArgumentException("Initial version tag cannot be empty.", nameof(initialVersionTag));

        var model = new AiModel
        {
            Id = Guid.NewGuid(),
            Name = name,
            ModelType = modelType,
            DeploymentStatus = DeploymentStatus.Pending
        };

        model.AddNewVersion(initialVersionTag, checksum);
        return model;
    }

    /// <summary>
    /// Adds a new version of the model. The new version becomes the current version.
    /// </summary>
    public void AddNewVersion(string versionTag, string checksum)
    {
        if (string.IsNullOrWhiteSpace(versionTag))
            throw new ArgumentException("Version tag cannot be empty.", nameof(versionTag));
        if (_versionHistory.Any(v => v.Tag == versionTag))
            throw new InvalidOperationException($"Version '{versionTag}' already exists for this model.");

        var newVersion = new ModelVersion(versionTag, DateTime.UtcNow, checksum);
        _versionHistory.Add(newVersion);
        CurrentVersion = newVersion;
        
        // When a new version is added, it should be considered pending deployment
        DeploymentStatus = DeploymentStatus.Pending;
    }

    /// <summary>
    /// Deploys the current version of the model.
    /// </summary>
    public void Deploy()
    {
        if (DeploymentStatus == DeploymentStatus.Archived)
        {
            throw new InvalidOperationException("Cannot deploy an archived model.");
        }
        DeploymentStatus = DeploymentStatus.Deployed;
        // In a real system, this would likely raise a ModelDeployedEvent
    }

    /// <summary>
    /// Updates the performance metrics of the model.
    /// </summary>
    public void UpdatePerformance(ModelPerformanceMetrics newMetrics)
    {
        PerformanceMetrics = newMetrics;
    }

    /// <summary>
    /// Archives the model, preventing future deployments.
    /// </summary>
    public void Archive()
    {
        DeploymentStatus = DeploymentStatus.Archived;
    }
}

#region Supporting Types

/// <summary>
/// Represents a specific version of an AI model artifact.
/// </summary>
public record ModelVersion(string Tag, DateTime UploadedAt, string Checksum);

/// <summary>
/// Encapsulates performance metrics for an AI model.
/// </summary>
public record ModelPerformanceMetrics(double Accuracy, double Precision, double Recall, DateTime LastEvaluated);

/// <summary>
/// Defines the types of AI models supported by the system.
/// </summary>
public enum ModelType
{
    PredictiveMaintenance,
    AnomalyDetection,
    Nlp
}

/// <summary>
/// Represents the deployment status of an AI model.
/// </summary>
public enum DeploymentStatus
{
    Pending,
    Deployed,
    Archived
}

#endregion