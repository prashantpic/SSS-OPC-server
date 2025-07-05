using MediatR;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;

namespace Opc.System.Services.AI.Application.Features.Nlq;

/// <summary>
/// Represents a request to process a natural language query.
/// </summary>
/// <param name="QueryText">The natural language query from the user.</param>
public record ProcessNlqCommand(string QueryText) : IRequest<NlqResultDto>;

/// <summary>
/// Data Transfer Object for the result of an NLQ process.
/// </summary>
/// <param name="Answer">A user-friendly text answer.</param>
/// <param name="Data">Structured data result, if applicable (e.g., a list of values).</param>
/// <param name="IsSuccess">Indicates if the query was successfully processed.</param>
public record NlqResultDto(string Answer, object? Data, bool IsSuccess);

/// <summary>
/// Handles the logic for processing a natural language query.
/// Orchestrates the process of interpreting and executing a natural language query.
/// </summary>
public class ProcessNlqCommandHandler : IRequestHandler<ProcessNlqCommand, NlqResultDto>
{
    private readonly INlpServiceFactory _nlpServiceFactory;
    private readonly INlqAliasRepository _aliasRepository;
    private readonly IDataServiceClient _dataServiceClient;

    public ProcessNlqCommandHandler(
        INlpServiceFactory nlpServiceFactory,
        INlqAliasRepository aliasRepository,
        IDataServiceClient dataServiceClient)
    {
        _nlpServiceFactory = nlpServiceFactory;
        _aliasRepository = aliasRepository;
        _dataServiceClient = dataServiceClient;
    }

    public async Task<NlqResultDto> Handle(ProcessNlqCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the configured NLP provider
            var nlpProvider = _nlpServiceFactory.GetProvider();

            // 2. Extract intent and entities from the query text
            var nlpResult = await nlpProvider.ExtractIntentAndEntitiesAsync(request.QueryText, cancellationToken);
            if (string.IsNullOrEmpty(nlpResult.Intent))
            {
                return new NlqResultDto("I'm sorry, I couldn't understand your request.", null, false);
            }

            // 3. Resolve aliases for entities
            // Example: "Boiler 1 Temperature" -> "ns=2;s=Boiler1.Temp"
            var resolvedEntities = new Dictionary<string, string>();
            foreach(var entity in nlpResult.Entities)
            {
                var resolvedValue = await _aliasRepository.ResolveAliasAsync(entity.Value, cancellationToken);
                resolvedEntities[entity.Key] = resolvedValue ?? entity.Value;
            }

            // 4. Based on intent, call the Data Service Client
            // This is a simplified example. A real implementation would be more robust.
            switch (nlpResult.Intent.ToLower())
            {
                case "get_value":
                    if(resolvedEntities.TryGetValue("tag", out var tagId))
                    {
                        var data = await _dataServiceClient.GetLatestValueAsync(tagId, cancellationToken);
                        return new NlqResultDto($"The current value for {tagId} is {data?.Value}.", data, true);
                    }
                    break;

                case "show_trend":
                     if(resolvedEntities.TryGetValue("tag", out var trendTagId))
                     {
                        // Assume time range entities are also extracted, e.g., "yesterday"
                        var historicalData = await _dataServiceClient.GetHistoricalDataAsync(trendTagId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, cancellationToken);
                        return new NlqResultDto($"Showing trend data for {trendTagId} for the last day.", historicalData, true);
                     }
                     break;
                
                default:
                    return new NlqResultDto($"I understood the intent '{nlpResult.Intent}', but I don't know how to handle it yet.", nlpResult, false);
            }

            return new NlqResultDto("I understood your request, but couldn't find the required information (e.g., a specific tag).", null, false);
        }
        catch (Exception ex)
        {
            // In a real app, log this exception
            return new NlqResultDto($"An error occurred while processing your request: {ex.Message}", null, false);
        }
    }
}