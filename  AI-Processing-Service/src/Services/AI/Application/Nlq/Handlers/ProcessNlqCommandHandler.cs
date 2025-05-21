using MediatR;
using AIService.Application.Nlq.Commands;
using AIService.Application.Nlq.Models;
using AIService.Domain.Interfaces;
using AIService.Domain.Models; // For NlqContext from Domain
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.Nlq.Handlers
{
    /// <summary>
    /// Processes the ProcessNlqCommand, interacts with NlpOrchestrationService
    /// to interpret the query, and returns the processed result.
    /// REQ-7-014: Integration with NLP Providers (handled by NlpOrchestrationService).
    /// REQ-7-016: Fallback NLQ Strategy (handled by NlpOrchestrationService).
    /// </summary>
    public class ProcessNlqCommandHandler : IRequestHandler<ProcessNlqCommand, NlqProcessingResult>
    {
        private readonly INlpOrchestrationService _nlpOrchestrationService;
        private readonly IMapper _mapper;
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
            _logger.LogInformation("Handling ProcessNlqCommand for Query: \"{QueryText}\", UserId: {UserId}", request.QueryText, request.UserId ?? "N/A");

            try
            {
                // Prepare context for the NlpOrchestrationService.
                // The domain's NlqContext might be richer than the command's ContextParameters.
                // For now, let's assume a direct mapping or that NlpOrchestrationService can take primitive types.
                // The SDS defines `NlqContext` in the domain layer.
                // The NlpOrchestrationService.ProcessAsync takes (string query, NlqContext context)
                // So, we should create or pass an NlqContext domain object.
                
                var initialDomainContext = new NlqContext(request.QueryText)
                {
                    UserId = request.UserId,
                    SessionId = request.SessionId,
                    // Additional custom parameters from request.ContextParameters can be added if NlqContext supports it
                    // e.g., initialDomainContext.UserParameters = request.ContextParameters;
                };
                 foreach(var param in request.ContextParameters ?? new Dictionary<string,string>())
                {
                    initialDomainContext.SetProperty(param.Key, param.Value);
                }


                NlqContext processedDomainContext = await _nlpOrchestrationService.ProcessAsync(request.QueryText, initialDomainContext, cancellationToken);

                // Map Domain.NlqContext to Application.NlqProcessingResult
                var nlqProcessingResult = _mapper.Map<NlqProcessingResult>(processedDomainContext);
                
                _logger.LogInformation("Successfully processed NLQ. Intent: {Intent}, Entities: {EntityCount}", nlqProcessingResult.Intent, nlqProcessingResult.Entities?.Count ?? 0);
                return nlqProcessingResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling ProcessNlqCommand for Query: \"{QueryText}\"", request.QueryText);
                // Consider returning a specific error structure in NlqProcessingResult
                // or rethrowing custom exceptions.
                throw;
            }
        }
    }
}