using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AIService.Application.PredictiveMaintenance.Commands;
using AIService.Application.PredictiveMaintenance.Models;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.PredictiveMaintenance.Handlers
{
    /// <summary>
    /// Processes the GetPredictionCommand, interacts with domain services (ModelExecutionService)
    /// to execute the relevant AI model, and returns the prediction result.
    /// REQ-7-001: Predictive Maintenance Analysis
    /// REQ-7-003: Execution of ONNX models (and other formats via ModelExecutionService)
    /// </summary>
    public class GetPredictionCommandHandler : IRequestHandler<GetPredictionCommand, PredictionOutput>
    {
        private readonly IModelExecutionService _modelExecutionService;
        private readonly IModelRepository _modelRepository; // To get model metadata if needed by ModelExecutionService
        private readonly IMapper _mapper;
        private readonly ILogger<GetPredictionCommandHandler> _logger;

        public GetPredictionCommandHandler(
            IModelExecutionService modelExecutionService,
            IModelRepository modelRepository,
            IMapper mapper,
            ILogger<GetPredictionCommandHandler> logger)
        {
            _modelExecutionService = modelExecutionService ?? throw new ArgumentNullException(nameof(modelExecutionService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PredictionOutput> Handle(GetPredictionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetPredictionCommand for ModelId: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion);

            try
            {
                // Potentially, ModelExecutionService could take ModelId and ModelVersion directly
                // and handle AiModel retrieval internally. For now, assuming it might need the AiModel object.
                // This logic might be more complex if a specific model isn't found or input validation against schema is done here.

                var modelInput = _mapper.Map<ModelInput>(request.InputData);
                if (modelInput == null)
                {
                    _logger.LogError("Failed to map InputData to ModelInput for ModelId: {ModelId}", request.ModelId);
                    return new PredictionOutput { Success = false, ErrorMessage = "Invalid input data format." };
                }
                
                // The ModelExecutionService is responsible for fetching the AiModel entity based on Id/Version,
                // loading the model artifact, and executing it.
                var modelOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, request.ModelVersion, modelInput);

                if (modelOutput == null || modelOutput.Outputs == null)
                {
                     _logger.LogWarning("Model execution for ModelId {ModelId} returned null or empty output.", request.ModelId);
                    return new PredictionOutput { Success = false, ErrorMessage = "Model execution failed or returned no output." };
                }
                
                var predictionOutput = _mapper.Map<PredictionOutput>(modelOutput);
                predictionOutput.Success = true;

                _logger.LogInformation("Successfully processed GetPredictionCommand for ModelId: {ModelId}", request.ModelId);
                return predictionOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling GetPredictionCommand for ModelId: {ModelId}", request.ModelId);
                return new PredictionOutput
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}