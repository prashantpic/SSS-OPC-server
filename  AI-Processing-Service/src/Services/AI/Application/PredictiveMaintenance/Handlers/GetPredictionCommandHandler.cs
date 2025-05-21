using MediatR;
using AutoMapper;
using AIService.Application.PredictiveMaintenance.Commands;
using AIService.Application.PredictiveMaintenance.Models;
using AIService.Domain.Services;
using AIService.Domain.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIService.Application.PredictiveMaintenance.Handlers
{
    /// <summary>
    /// Processes the GetPredictionCommand, interacts with domain services (ModelExecutionService)
    /// to execute the relevant AI model, and returns the prediction result.
    /// REQ-7-001: Predictive Maintenance Analysis
    /// REQ-7-003: Execution of ONNX models (and other formats via IModelExecutionService)
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
            _logger.LogInformation("Handling GetPredictionCommand for ModelId: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion);

            // In a real scenario, ModelInput would be a more structured domain object,
            // potentially created with validation against the model's input schema.
            var modelInput = new ModelInput { Features = request.InputData };

            try
            {
                ModelOutput domainOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, request.ModelVersion, modelInput);
                
                // Map the domain model output to the application model output
                PredictionOutput applicationOutput = _mapper.Map<PredictionOutput>(domainOutput);
                applicationOutput.Success = true; // Assuming execution implies success if no exception
                
                _logger.LogInformation("Successfully executed prediction for ModelId: {ModelId}", request.ModelId);
                return applicationOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing prediction for ModelId: {ModelId}", request.ModelId);
                // Return a PredictionOutput indicating failure
                return new PredictionOutput
                {
                    Success = false,
                    ErrorMessage = $"Failed to get prediction: {ex.Message}",
                    // Populate other fields as appropriate for an error case
                };
            }
        }
    }
}