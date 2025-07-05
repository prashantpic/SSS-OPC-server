using Azure.AI.Language.QuestionAnswering;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;
using Microsoft.Extensions.Configuration;

namespace Opc.System.Services.AI.Infrastructure.Nlp;

// NOTE: The following interfaces are defined here to satisfy dependencies without creating unlisted files.
// Ideally, each would be in its own file in the Application layer.
namespace Opc.System.Services.AI.Application.Interfaces
{
    using Microsoft.Extensions.DependencyInjection;
    
    public interface INlpServiceProvider
    {
        Task<NlpResult> ExtractIntentAndEntitiesAsync(string queryText, CancellationToken cancellationToken = default);
    }

    public interface INlpServiceFactory
    {
        INlpServiceProvider GetProvider();
    }

    // A concrete factory implementation
    public class NlpServiceFactory : INlpServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NlpServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INlpServiceProvider GetProvider()
        {
            // This could be enhanced with feature flags to switch between providers
            return _serviceProvider.GetRequiredService<INlpServiceProvider>();
        }
    }
}

namespace Opc.System.Services.AI.Application.Interfaces.Models
{
    public record NlpResult(string Intent, IReadOnlyDictionary<string, string> Entities);
}
// End of embedded interface definitions.

namespace Opc.System.Services.AI.Infrastructure.Nlp
{
    /// <summary>
    /// Implements the NLP service provider contract using Azure Cognitive Service for Language.
    /// Adapts requests from the application layer to the specific API of Azure's service.
    /// NOTE: This uses QuestionAnswering as a stand-in for Conversational Language Understanding (CLU)
    /// for simplicity, as described in the SDS. A production system should use CLU.
    /// </summary>
    public class AzureNlpServiceProvider : INlpServiceProvider
    {
        private readonly QuestionAnsweringClient _client;
        private readonly IConfiguration _configuration;

        public AzureNlpServiceProvider(QuestionAnsweringClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task<NlpResult> ExtractIntentAndEntitiesAsync(string queryText, CancellationToken cancellationToken = default)
        {
            var projectName = _configuration["Azure:CognitiveServices:ProjectName"];
            var deploymentName = _configuration["Azure:CognitiveServices:DeploymentName"];
            
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(deploymentName))
            {
                throw new InvalidOperationException("Azure Cognitive Services ProjectName and DeploymentName must be configured.");
            }

            var project = new QuestionAnsweringProject(projectName, deploymentName);
            Response<AnswersResult> response = await _client.GetAnswersAsync(queryText, project, cancellationToken: cancellationToken);

            if (response.Value.Answers.Any())
            {
                var topAnswer = response.Value.Answers.First();

                // This is a simulation. We extract intent and entities from the answer's metadata.
                // A real CLU response would have direct `Intent` and `Entities` properties.
                string intent = "unknown";
                var entities = new Dictionary<string, string>();

                if (topAnswer.Metadata.TryGetValue("intent", out var intentValue))
                {
                    intent = intentValue;
                }
                foreach (var meta in topAnswer.Metadata)
                {
                    if (meta.Key != "intent")
                    {
                        entities[meta.Key] = meta.Value;
                    }
                }
                
                // If no intent in metadata, we can infer it from the question's structure
                if (intent == "unknown")
                {
                    if (queryText.ToLower().Contains("average") || queryText.ToLower().Contains("mean")) intent = "get_average";
                    else if (queryText.ToLower().Contains("what is") || queryText.ToLower().Contains("current")) intent = "get_value";
                    else if (queryText.ToLower().Contains("trend") || queryText.ToLower().Contains("history")) intent = "show_trend";
                }

                return new NlpResult(intent, entities);
            }

            return new NlpResult("unknown", new Dictionary<string, string>());
        }
    }
}