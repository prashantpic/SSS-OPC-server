using AiService.Application.Dtos;
using AiService.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiService.Application.Features.Nlq.Queries.ProcessNlq;

/// <summary>
/// Orchestrates NLQ processing by integrating with NLP and Data services.
/// This handler translates natural language into structured data queries.
/// </summary>
public class ProcessNlqQueryHandler : IRequestHandler<ProcessNlqQuery, NlqResultDto>
{
    private readonly INlpServiceProvider _nlpServiceProvider;
    private readonly IDataServiceClient _dataServiceClient;
    private readonly ILogger<ProcessNlqQueryHandler> _logger;

    public ProcessNlqQueryHandler(
        INlpServiceProvider nlpServiceProvider, 
        IDataServiceClient dataServiceClient,
        ILogger<ProcessNlqQueryHandler> logger)
    {
        _nlpServiceProvider = nlpServiceProvider;
        _dataServiceClient = dataServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ProcessNlqQuery.
    /// </summary>
    /// <param name="request">The query containing the natural language text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DTO containing the processed result.</returns>
    public async Task<NlqResultDto> Handle(ProcessNlqQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling NLQ query for user {UserId}: '{QueryText}'", request.UserId, request.QueryText);

        try
        {
            // 1. Interpret the natural language query
            var structuredResult = await _nlpServiceProvider.InterpretQueryAsync(request.QueryText, cancellationToken);
            _logger.LogInformation("NLQ interpretation result - Intent: {Intent}, Confidence: {Confidence}", structuredResult.Intent, structuredResult.Confidence);

            if (structuredResult.Confidence < 0.7) // Configurable threshold
            {
                _logger.LogWarning("NLP confidence is below threshold. Returning clarification.");
                return new NlqResultDto(
                    request.QueryText,
                    "Interpretation uncertain",
                    "ClarificationNeeded",
                    "I'm not sure what you mean. Could you please rephrase your question?"
                );
            }

            // 2. Fetch data using the structured interpretation
            var data = await _dataServiceClient.FetchDataFromNlpResultAsync(structuredResult, cancellationToken);
            _logger.LogInformation("Data service returned data for NLQ query.");

            // 3. Format and return the result
            return new NlqResultDto(
                request.QueryText,
                $"Intent: {structuredResult.Intent}, Entities: {string.Join(", ", structuredResult.Entities.Select(e => $"{e.Key}={e.Value}"))}",
                "Success",
                data
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing NLQ query for user {UserId}", request.UserId);
            return new NlqResultDto(
                request.QueryText,
                "Error processing query",
                "Error",
                "An internal error occurred while trying to process your request."
            );
        }
    }
}