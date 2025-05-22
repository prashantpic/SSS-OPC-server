using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
// For ONNXRuntime.InferenceSession, if needed directly in domain model (usually better in infrastructure)
// using Microsoft.ML.OnnxRuntime; 

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Represents an AI model loaded and ready for execution on the edge client, 
/// including its configuration and path to the model file.
/// </summary>
public class EdgeAiModel
{
    public required EdgeModelMetadataDto Metadata { get; set; }
    public required string FullFilePath { get; set; }
    public bool IsLoaded { get; set; } = false;
    public string? LastError { get; set; }

    // The InferenceSession could be here if the domain needs to know about it,
    // but it's often better to keep ONNX-specifics in the infrastructure layer
    // and have the IEdgeAiExecutor manage the session.
    // public InferenceSession? InferenceSession { get; set; }

    public EdgeAiModel() { } // For frameworks or manual instantiation
}