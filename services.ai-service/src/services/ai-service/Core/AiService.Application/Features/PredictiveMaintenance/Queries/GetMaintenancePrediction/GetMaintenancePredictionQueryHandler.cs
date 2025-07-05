using AiService.Application.Dtos;
using AiService.Application.Interfaces.Infrastructure;
using AiService.Domain.Enums;
using AiService.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiService.Application.Features.PredictiveMaintenance.Queries.GetMaintenancePrediction;

/// <summary>
/// The handler responsible for orchestrating the execution of a predictive maintenance model.
/// It processes prediction requests by fetching model details and invoking the ML engine.
/// </summary>
public class GetMaintenancePredictionQueryHandler : IRequestHandler<GetMaintenancePredictionQuery, PredictionResultDto>
{
    private readonly IAiModelRepository _modelRepository;
    private readonly IModelArtifactStorage _artifactStorage;
    private readonly IPredictionEngine _predictionEngine;
    private readonly ILogger<GetMaintenancePredictionQueryHandler> _logger;

    public GetMaintenancePredictionQueryHandler(
        IAiModelRepository modelRepository, 
        IModelArtifactStorage artifactStorage, 
        IPredictionEngine predictionEngine, 
        ILogger<GetMaintenancePredictionQueryHandler> logger)
    {
        _modelRepository = modelRepository;
        _artifactStorage = artifactStorage;
        _predictionEngine = predictionEngine;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetMaintenancePredictionQuery.
    /// </summary>
    /// <param name="request">The query containing the model ID and input data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DTO containing the prediction result.</returns>
    /// <exception cref="ArgumentException">Thrown when the model is not found, not deployed, or misconfigured.</exception>
    public async Task<PredictionResultDto> Handle(GetMaintenancePredictionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling predictive maintenance query for model {ModelId}", request.ModelId);

        var model = await _modelRepository.GetByIdAsync(request.ModelId, cancellationToken);

        if (model is null)
        {
            _logger.LogWarning("Model with ID {ModelId} not found.", request.ModelId);
            throw new ArgumentException($"Model with ID {request.ModelId} not found.", nameof(request.ModelId));
        }

        if (model.Status != ModelStatus.Deployed)
        {
            _logger.LogWarning("Model {ModelId} is not deployed. Current status: {Status}", model.Id, model.Status);
            throw new InvalidOperationException($"Model '{model.Name}' is not deployed.");
        }
        
        if (model.ModelType != ModelType.PredictiveMaintenance)
        {
             _logger.LogWarning("Model {ModelId} is not a predictive maintenance model. Type: {Type}", model.Id, model.ModelType);
            throw new InvalidOperationException($"Model '{model.Name}' is not a predictive maintenance model.");
        }

        var artifact = model.Artifacts.FirstOrDefault();
        if (artifact is null)
        {
            _logger.LogError("Deployed model {ModelId} has no associated artifact.", model.Id);
            throw new InvalidOperationException($"Model '{model.Name}' is deployed but has no artifact.");
        }

        try
        {
            _logger.LogInformation("Retrieving artifact stream from path: {StoragePath}", artifact.StoragePath);
            await using var modelStream = await _artifactStorage.GetModelStreamAsync(artifact.StoragePath, cancellationToken);
            
            _logger.LogInformation("Running prediction for model {ModelId}", request.ModelId);
            var result = await _predictionEngine.RunPredictionAsync(modelStream, request.InputData, cancellationToken);
            
            _logger.LogInformation("Prediction successful for model {ModelId}", request.ModelId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during prediction for model {ModelId}", request.ModelId);
            throw; // Re-throw to be handled by global exception middleware
        }
    }
}