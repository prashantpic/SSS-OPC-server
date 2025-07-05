using AiService.Application.Dtos;
using MediatR;

namespace AiService.Application.Features.Nlq.Queries.ProcessNlq;

/// <summary>
/// Input for the NLQ processing feature.
/// Represents a user's natural language query to be processed.
/// </summary>
/// <param name="QueryText">The natural language query text from the user.</param>
/// <param name="UserId">The unique identifier of the user submitting the query.</param>
public record ProcessNlqQuery(string QueryText, Guid UserId) : IRequest<NlqResultDto>;