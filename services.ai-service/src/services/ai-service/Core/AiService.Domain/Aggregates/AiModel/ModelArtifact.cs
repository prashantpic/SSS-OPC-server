namespace AiService.Domain.Aggregates.AiModel;

/// <summary>
/// An entity within the AiModel aggregate that represents a specific file artifact of the model, such as an ONNX file.
/// Its purpose is to track the location and integrity of a trained model's file.
/// </summary>
public class ModelArtifact
{
    /// <summary>
    /// The unique identifier for the model artifact.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The path to the artifact in the configured blob storage.
    /// </summary>
    public string StoragePath { get; private set; }

    /// <summary>
    /// A checksum (e.g., SHA256) of the artifact file to verify its integrity.
    /// </summary>
    public string Checksum { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelArtifact"/> class.
    /// </summary>
    /// <param name="storagePath">The path to the artifact in storage.</param>
    /// <param name="checksum">The checksum of the artifact file.</param>
    public ModelArtifact(string storagePath, string checksum)
    {
        Id = Guid.NewGuid();
        StoragePath = !string.IsNullOrWhiteSpace(storagePath) ? storagePath : throw new ArgumentNullException(nameof(storagePath));
        Checksum = !string.IsNullOrWhiteSpace(checksum) ? checksum : throw new ArgumentNullException(nameof(checksum));
    }

    // Private constructor for ORM
#pragma warning disable CS8618
    private ModelArtifact() { }
#pragma warning restore CS8618
}