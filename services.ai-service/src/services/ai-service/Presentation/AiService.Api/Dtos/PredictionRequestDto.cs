using System.ComponentModel.DataAnnotations;

namespace AiService.Api.Dtos;

/// <summary>
/// Represents the body of a request to get a maintenance prediction.
/// </summary>
public class PredictionRequestDto
{
    /// <summary>
    /// A dictionary containing the input feature names and their corresponding values for the prediction.
    /// The keys and the number of elements must match the model's expectations.
    /// </summary>
    [Required]
    public Dictionary<string, float> InputData { get; set; } = new();
}