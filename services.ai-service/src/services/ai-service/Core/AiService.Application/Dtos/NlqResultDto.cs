namespace AiService.Application.Dtos;

/// <summary>
/// Data Transfer Object representing the result of a Natural Language Query processing.
/// </summary>
/// <param name="OriginalQuery">The original query text submitted by the user.</param>
/// <param name="InterpretedQuery">A system-generated interpretation of the user's query.</param>
/// <param name="ResultType">The type of result data, e.g., 'TimeSeries', 'SingleValue', 'ClarificationNeeded'.</param>
/// <param name="Data">The actual data retrieved, or clarification questions.</param>
public record NlqResultDto(string OriginalQuery, string InterpretedQuery, string ResultType, object Data);