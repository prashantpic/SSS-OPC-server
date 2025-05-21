using MediatR;
using AIService.Application.PredictiveMaintenance.Commands;
using AIService.Application.PredictiveMaintenance.Models;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.PredictiveMaintenance.Handlers
{
    /// <summary>
    /// Processes the GetPredictionCommand, interacts with ModelExecutionService 
    /// to execute the relevant AI model, and returns the prediction result.
    /// REQ-7-001: Core functionality for predictive maintenance.
    /// REQ-7-003: Execution of ONNX models (handled by ModelExecutionService).
    /// </summary>
    public class GetPredictionCommandHandler : IRequestHandler<GetPredictionCommand, PredictionOutput>
    {
        private readonly IModelExecutionService _modelExecutionService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPredictionCommandHandler> _logger;

        public GetPredictionCommandHandler(
            IModelExecutionService modelExecutionService,
            IMapper mapper,
            ILogger<GetPredictionCommandHandler> logger)
        {
            _modelExecutionService = modelExecutionService ?? throw new ArgumentNullException(nameof(modelExecutionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PredictionOutput> Handle(GetPredictionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetPredictionCommand for ModelId: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion ?? "Latest");

            try
            {
                // Assuming ModelInput in Domain layer takes a dictionary or can be mapped from it.
                // The actual structure of ModelInput might be more complex and mapping more sophisticated.
                var modelInput = _mapper.Map<ModelInput>(request.Features);
                // If schema validation is needed before calling domain service, it can be done here or within ModelExecutionService
                // For example, by fetching AiModel.InputSchema via IModelRepository first.

                ModelOutput modelOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, modelInput, request.ModelVersion, cancellationToken);

                // Map Domain.ModelOutput to Application.PredictionOutput
                var predictionOutput = _mapper.Map<PredictionOutput>(modelOutput);
                
                _logger.LogInformation("Successfully processed GetPredictionCommand for ModelId: {ModelId}", request.ModelId);
                return predictionOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling GetPredictionCommand for ModelId: {ModelId}", request.ModelId);
                // Consider custom exceptions for different failure scenarios (e.g., ModelNotFoundException, ModelExecutionException)
                // For now, rethrow or return a specific error structure in PredictionOutput
                throw; 
            }
        }
    }
}