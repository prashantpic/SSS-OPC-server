using System.ComponentModel.DataAnnotations;

namespace AiService.Api.Dtos;

/// <summary>
/// Represents the body of a request to process a Natural Language Query.
/// </summary>
public class NlqRequestDto
{
    /// <summary>
    /// The natural language query text submitted by the user.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string QueryText { get; set; } = string.Empty;
}