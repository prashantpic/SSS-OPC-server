using MediatR;
using AutoMapper;
using AIService.Application.Nlq.Commands;
using AIService.Application.Nlq.Models;
using AIService.Domain.Services;
using AIService.Domain.Models; // For NlqContext
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // For Dictionary

namespace AIService.Application.Nlq.Handlers
{
    /// <summary>
    /// Processes the ProcessNlqCommand, interacts with domain services (NlpOrchestrationService),
    /// and returns the interpretation or data.
    /// REQ-7-014: Integration with NLP Providers
    /// REQ-7-016: Fallback NLQ Strategy
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
            _logger.LogInformation("Handling ProcessNlqCommand for Query: {QueryText}", request.QueryText);

            // Map command context to domain NlqContext if necessary, or pass relevant parts.
            // For simplicity, assuming NlpOrchestrationService takes basic types and constructs NlqContext internally or accepts it.
            // A more robust approach would be to map request.ContextData to NlqContext.UserDefinedAliases or similar.
            var domainNlqContext = new NlqContext(request.QueryText);
            if (request.ContextData != null)
            {
                foreach(var item in request.ContextData)
                {
                    // This is a simplistic mapping. NlqContext might have a more structured way to handle this.
                    domainNlqContext.AppliedAliases.TryAdd(item.Key, item.Value); 
                }
            }


            try
            {
                NlqContext processedContext = await _nlpOrchestrationService.ProcessAsync(request.QueryText, domainNlqContext);
                
                // Map the domain NlqContext to the application model NlqProcessingResult
                NlqProcessingResult applicationResult = _mapper.Map<NlqProcessingResult>(processedContext);
                applicationResult.Success = true; // Assuming processing implies success if no exception

                _logger.LogInformation("Successfully processed NLQ: {QueryText}", request.QueryText);
                return applicationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NLQ: {QueryText}", request.QueryText);
                return new NlqProcessingResult
                {
                    Success = false,
                    OriginalQuery = request.QueryText,
                    Message = $"Failed to process NLQ: {ex.Message}"
                };
            }
        }
    }

    // Define placeholder for NlqProcessingResult if not already defined elsewhere
    // These would typically be in AIService.Application.Nlq.Models
    namespace AIService.Application.Nlq.Models
    {
        public class NlqProcessingResult
        {
            public bool Success { get; set; }
            public string OriginalQuery { get; set; }
            public string ProcessedQuery { get; set; }
            public string Intent { get; set; }
            public Dictionary<string, string> Entities { get; set; } // Simplified: EntityType -> Value
            public double Confidence { get; set; }
            public string Message { get; set; }
            public bool FallbackApplied { get; set; }


            public NlqProcessingResult()
            {
                Entities = new Dictionary<string, string>();
            }
        }
    }
}