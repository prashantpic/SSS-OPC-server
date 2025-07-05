using MediatR;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;
using Opc.System.Services.AI.Domain.Aggregates;
using Opc.System.Services.AI.Domain.Interfaces;

namespace Opc.System.Services.AI.Application.Features.PredictiveMaintenance;

/// <summary>
/// Represents the command to run a predictive maintenance analysis.
/// </summary>
/// <param name="AssetId">The ID of the asset to analyze.</param>
/// <param name="ModelId">The ID of the model to use for the prediction.</param>
public record RunPredictionCommand(Guid AssetId, Guid ModelId) : IRequest<PredictionResultDto>;

/// <summary>
/// Data Transfer Object for the prediction result.
/// </summary>
/// <param name="IsFailurePredicted">Whether a failure is predicted.</param>
/// <param name="Confidence">The confidence score of the prediction (0.0 to 1.0).</param>
/// <param name="PredictedFailureDate">The estimated date of failure, if applicable.</param>
public record PredictionResultDto(bool IsFailurePredicted, double Confidence, DateTime? PredictedFailureDate);

/// <summary>
/// Handles the command to run a predictive maintenance analysis.
/// Orchestrates the execution of a predictive maintenance model.
/// </summary>
public class RunPredictionCommandHandler : IRequestHandler<RunPredictionCommand, PredictionResultDto>
{
    private readonly IAiModelRepository _modelRepository;
    private readonly IModelRunner _modelRunner;
    private readonly IDataServiceClient _dataServiceClient;

    public RunPredictionCommandHandler(
        IAiModelRepository modelRepository,
        IModelRunner modelRunner,
        IDataServiceClient dataServiceClient)
    {
        _modelRepository = modelRepository;
        _modelRunner = modelRunner;
        _dataServiceClient = dataServiceClient;
    }

    public async Task<PredictionResultDto> Handle(RunPredictionCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the AI model metadata
        var model = await _modelRepository.GetByIdAsync(request.ModelId, cancellationToken);

        if (model is null)
        {
            throw new ApplicationException($"Model with ID {request.ModelId} not found.");
        }
        if (model.ModelType != ModelType.PredictiveMaintenance || model.DeploymentStatus != DeploymentStatus.Deployed)
        {
            throw new InvalidOperationException($"Model {request.ModelId} is not a deployed predictive maintenance model.");
        }

        // 2. Fetch the required historical data for the asset
        // The specific tags and time range would be defined by the model's requirements,
        // which could be stored as metadata with the AiModel aggregate.
        var historicalData = await _dataServiceClient.GetHistoricalDataForAssetAsync(request.AssetId, cancellationToken);
        if (!historicalData.Any())
        {
            throw new ApplicationException($"No historical data found for asset {request.AssetId}.");
        }

        // 3. Prepare the input for the model runner
        var modelInput = new ModelInputData(new Dictionary<string, object>
        {
            { "timeseries_input", historicalData }
        });

        // 4. Load the model artifact and run inference
        var modelOutput = await _modelRunner.RunPredictionAsync(model.Id, model.CurrentVersion.Tag, modelInput, cancellationToken);
        
        // 5. Map the model output to the result DTO
        // The output keys ('prediction', 'confidence') are specific to the ONNX model's output nodes.
        var isFailurePredicted = Convert.ToBoolean(modelOutput.Outputs["prediction"]);
        var confidence = Convert.ToDouble(modelOutput.Outputs["confidence"]);
        
        DateTime? failureDate = null;
        if (isFailurePredicted && modelOutput.Outputs.TryGetValue("predicted_failure_date", out var dateValue))
        {
            failureDate = Convert.ToDateTime(dateValue);
        }

        return new PredictionResultDto(isFailurePredicted, confidence, failureDate);
    }
}