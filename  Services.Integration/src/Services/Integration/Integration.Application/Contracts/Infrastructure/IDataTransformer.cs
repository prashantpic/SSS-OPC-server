namespace Opc.System.Services.Integration.Application.Contracts.Infrastructure;

/// <summary>
/// Defines the contract for a data transformation service.
/// </summary>
public interface IDataTransformer
{
    /// <summary>
    /// Transforms a source data payload into a target format based on a specified data map.
    /// </summary>
    /// <param name="dataMapId">The unique identifier of the DataMap to use for the transformation rules.</param>
    /// <param name="sourcePayload">The source data payload as a string (e.g., JSON).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transformed payload as a string.</returns>
    Task<string> TransformAsync(Guid dataMapId, string sourcePayload);
}