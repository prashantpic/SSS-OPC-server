using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AIService.Application.Nlq.Commands;
using AIService.Domain.Interfaces;
using AIService.Domain.Models; // For NlqContext
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.Nlq.Handlers
{
    /// <summary>
    /// Processes the ProcessNlqCommand, interacts with domain services (NlpOrchestrationService),
    /// and returns the interpretation or data.
    /// REQ-7-014: Integration with NLP Providers
    /// REQ-7-016: Fallback NLQ Strategy (handled by NlpOrchestrationService)
    /// </summary>
    public class ProcessNlqCommandHandler : IRequestHandler<ProcessNlqCommand, NlqProcessingResult>
    {
        private readonly INlpOrchestrationService _nlpOrchestrationService;
        private readonly IMapper _mapper; // Potentially for mapping command to an initial NlqContext if needed
        private readonly ILogger<ProcessNlqCommandHandler> _logger;

        public ProcessNlqCommandHandler(
            INlpOrchestrationService nlpOrchestrationService,
            IMapper mapper,
            ILogger<ProcessNlqCommandHandler> logger)
        {
            _nlpOrchestrationService = nlpOrchestrationService ?? throw new ArgumentNullException(nameof(nlpOrchestrationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NlqProcessingResult> Handle(ProcessNlqCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling ProcessNlqCommand for Query: '{QueryText}', UserId: {UserId}, SessionId: {SessionId}", 
                request.QueryText, request.UserId, request.SessionId);

            try
            {
                // Initialize NlqContext. If PreviousContext is provided, use it as a base.
                // Otherwise, create a new one.
                NlqContext contextToProcess;
                if (request.PreviousContext != null)
                {
                    contextToProcess = request.PreviousContext;
                    // Update context with new query if necessary, NlpOrchestrationService might handle this
                    contextToProcess.OriginalQuery = request.QueryText; 
                }
                else
                {
                    // Here, we could use AutoMapper to map from ProcessNlqCommand to NlqContext
                    // if there are more fields to transfer, or create it manually.
                    contextToProcess = new NlqContext(request.QueryText)
                    {
                        UserId = request.UserId,
                        SessionId = request.SessionId
                        // Other initializations like user-defined aliases REQ-7-015 would be loaded by NlpOrchestrationService
                    };
                }
                
                var processedContext = await _nlpOrchestrationService.ProcessAsync(contextToProcess);

                if (processedContext == null)
                {
                     _logger.LogWarning("NLQ processing for Query '{QueryText}' returned null context.", request.QueryText);
                    return new NlqProcessingResult { Success = false, ErrorMessage = "NLQ processing failed or returned no context." };
                }

                _logger.LogInformation("Successfully processed NLQ: '{QueryText}'. Intent: {Intent}, Entities: {EntityCount}",
                    request.QueryText, processedContext.IdentifiedIntent, processedContext.ExtractedEntities?.Count ?? 0);

                return new NlqProcessingResult { Success = true, ProcessedContext = processedContext };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling ProcessNlqCommand for Query: '{QueryText}'", request.QueryText);
                return new NlqProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred during NLQ processing: {ex.Message}"
                };
            }
        }
    }
}