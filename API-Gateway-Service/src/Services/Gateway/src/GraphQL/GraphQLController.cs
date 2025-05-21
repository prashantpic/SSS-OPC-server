using GraphQL; // IDocumentExecuter
using GraphQL.Types; // ISchema
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GatewayService.GraphQL
{
    /// <summary>
    /// HTTP endpoint for GraphQL queries and mutations.
    /// Provides an HTTP endpoint (e.g., /graphql) for receiving and processing
    /// GraphQL queries and mutations. It uses the GraphQL.NET library to parse,
    /// validate, and execute requests against the defined schema.
    /// </summary>
    [ApiController]
    [Route("graphql")] // As per SDS and common practice
    public class GraphQLController : ControllerBase
    {
        private readonly IDocumentExecuter _documentExecuter;
        private readonly ISchema _schema;
        private readonly ILogger<GraphQLController> _logger;

        public GraphQLController(
            IDocumentExecuter documentExecuter,
            ISchema schema,
            ILogger<GraphQLController> logger)
        {
            _documentExecuter = documentExecuter ?? throw new ArgumentNullException(nameof(documentExecuter));
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a GraphQL query.
        /// </summary>
        /// <param name="query">The GraphQL query parameters.</param>
        /// <returns>The result of the GraphQL execution.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
        {
            if (query == null)
            {
                _logger.LogWarning("GraphQL query received was null.");
                return BadRequest("A GraphQL query must be provided.");
            }

            if (string.IsNullOrWhiteSpace(query.Query))
            {
                 _logger.LogWarning("GraphQL query string was null or empty.");
                return BadRequest("The 'query' field in the GraphQL request cannot be null or empty.");
            }
            
            _logger.LogDebug("Executing GraphQL query: {Query}", query.Query);

            var executionOptions = new ExecutionOptions
            {
                Schema = _schema,
                Query = query.Query,
                OperationName = query.OperationName,
                Variables = query.Variables?.ToInputs(), // GraphQL.NET helper for converting JSON to Inputs
                UserContext = User, // Pass HttpContext.User for auth purposes within resolvers
                RequestServices = HttpContext.RequestServices // Make DI services available to resolvers
            };

            try
            {
                var result = await _documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

                if (result.Errors?.Count > 0)
                {
                    _logger.LogError("GraphQL execution completed with errors: {Errors}", JsonSerializer.Serialize(result.Errors));
                    // Typically, GraphQL still returns 200 OK even with errors, with errors detailed in the response body.
                    // Some prefer to return 400 if there are only errors and no data.
                    // For simplicity, following common practice of 200 OK with errors in payload.
                }
                else
                {
                    _logger.LogDebug("GraphQL execution successful.");
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during GraphQL execution for query: {Query}", query.Query);
                // Return a structured error that GraphQL clients can understand
                var errorResult = new ExecutionResult
                {
                    Errors = new ExecutionErrors { new ExecutionError("An internal server error occurred.", ex) }
                };
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResult);
            }
        }
    }

    /// <summary>
    /// Represents a typical GraphQL request payload.
    /// </summary>
    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public JsonElement? Variables { get; set; } // Using JsonElement to be flexible with variables structure
    }
}