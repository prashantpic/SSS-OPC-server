using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // If you want to authorize the entire endpoint

namespace GatewayService.GraphQL
{
    /// <summary>
    /// HTTP endpoint for GraphQL queries and mutations.
    /// Provides an HTTP endpoint (e.g., /graphql) for receiving and processing GraphQL queries and mutations. 
    /// It uses the GraphQL.NET library to parse, validate, and execute requests against the defined schema.
    /// </summary>
    [ApiController]
    [Route("[controller]")] // Conventionally /graphql
    // [Authorize] // Uncomment to secure the entire GraphQL endpoint
    public class GraphQLController : ControllerBase
    {
        private readonly IDocumentExecuter _documentExecuter;
        private readonly ISchema _schema;
        private readonly ILogger<GraphQLController> _logger;
        // private readonly IDataLoaderContextAccessor _dataLoaderContextAccessor; // Optional for data loaders

        public GraphQLController(
            IDocumentExecuter documentExecuter,
            ISchema schema,
            ILogger<GraphQLController> logger)
            // IDataLoaderContextAccessor dataLoaderContextAccessor) // Optional
        {
            _documentExecuter = documentExecuter ?? throw new ArgumentNullException(nameof(documentExecuter));
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _dataLoaderContextAccessor = dataLoaderContextAccessor; // Optional
        }

        /// <summary>
        /// Executes a GraphQL query.
        /// </summary>
        /// <param name="request">The GraphQL request containing the query, variables, and operation name.</param>
        /// <returns>The result of the GraphQL execution.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] GraphQLRequest request)
        {
            if (request == null)
            {
                return BadRequest("GraphQL request cannot be null.");
            }
            _logger.LogInformation("Received GraphQL request. OperationName: {OperationName}", request.OperationName);
            _logger.LogTrace("GraphQL Query: {Query}, Variables: {Variables}", request.Query, request.Variables);


            var executionOptions = new ExecutionOptions
            {
                Schema = _schema,
                Query = request.Query,
                OperationName = request.OperationName,
                Variables = request.Variables, // Directly use the JObject or Dictionary<string, object>
                UserContext = User, // Pass HttpContext.User for authorization within resolvers
                RequestServices = HttpContext.RequestServices, // Make DI services available to resolvers
                CancellationToken = HttpContext.RequestAborted,
                // EnableMetrics = true, // Optional: for performance monitoring
                // ThrowOnUnhandledException = true // Optional: Recommended for development
            };
            
            // Example of how to use DataLoader if it was configured
            // if (_dataLoaderContextAccessor != null)
            // {
            //    executionOptions.Listeners.Add(new DataLoaderDocumentListener(_dataLoaderContextAccessor));
            // }


            var result = await _documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

            if (result.Errors?.Count > 0)
            {
                _logger.LogError("GraphQL execution completed with errors: {Errors}", result.Errors);
                return BadRequest(result); // Or Ok(result) as GraphQL spec often returns 200 OK with errors object
            }
            
            _logger.LogInformation("GraphQL execution successful. OperationName: {OperationName}", request.OperationName);
            return Ok(result);
        }
    }

    /// <summary>
    /// Represents a GraphQL request payload.
    /// </summary>
    public class GraphQLRequest
    {
        public string? OperationName { get; set; }
        public string Query { get; set; } = string.Empty;
        public Newtonsoft.Json.Linq.JObject? Variables { get; set; } // Using JObject for flexibility with GraphQL.SystemTextJson
        // Or use: public Dictionary<string, object>? Variables { get; set; } if using GraphQL.NewtonsoftJson
    }
}