using Opc.Client.Core.Domain.ValueObjects;
using Opc.Ua;
using Opc.Ua.Client;

namespace Opc.Client.Core.Infrastructure.Opc.Protocols;

/// <summary>
/// A concrete client for querying historical data from OPC HDA servers.
/// </summary>
/// <remarks>
/// This implementation targets OPC UA Historical Access (HDA) profiles. It translates
/// domain-specific queries into UA HistoryRead service calls.
/// </remarks>
public class OpcHdaClient
{
    private readonly Session _session;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcHdaClient"/> class.
    /// </summary>
    /// <param name="session">An active, connected OPC UA session.</param>
    public OpcHdaClient(Session session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Asynchronously queries historical data based on the provided query parameters.
    /// </summary>
    /// <param name="query">The historical data query definition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of historical tag values.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the session is not connected.</exception>
    public Task<IEnumerable<TagValue>> QueryHistoricalDataAsync(HistoricalDataQuery query, CancellationToken cancellationToken)
    {
        if (!_session.Connected)
            throw new InvalidOperationException("OPC UA Session is not connected.");

        var nodesToRead = new HistoryReadValueIdCollection(query.NodeIds.Select(nodeId => new HistoryReadValueId
        {
            NodeId = new NodeId(nodeId)
        }));

        HistoryReadDetails details = query.AggregationType switch
        {
            HdaAggregationType.Raw => new ReadRawModifiedDetails
            {
                IsReadModified = false,
                StartTime = query.StartTime,
                EndTime = query.EndTime,
                NumValuesPerNode = 0,
                ReturnBounds = true
            },
            _ => new ReadProcessedDetails
            {
                StartTime = query.StartTime,
                EndTime = query.EndTime,
                ProcessingInterval = query.ProcessingInterval ?? 0,
                AggregateType = new NodeIdCollection(new[] { GetAggregateNodeId(query.AggregationType) })
            }
        };

        _session.HistoryRead(
            null,
            new ExtensionObject(details),
            TimestampsToReturn.Source,
            false,
            nodesToRead,
            out var results,
            out var diagnosticInfos);
        
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        var allValues = new List<TagValue>();
        foreach (var result in results)
        {
            if (StatusCode.IsBad(result.StatusCode)) continue;

            if (result.HistoryData.Body is HistoryData data)
            {
                allValues.AddRange(data.DataValues.Select(dv => new TagValue(
                    dv.Value,
                    StatusCode.IsGood(dv.StatusCode) ? OpcQuality.Good : StatusCode.IsUncertain(dv.StatusCode) ? OpcQuality.Uncertain : OpcQuality.Bad,
                    dv.SourceTimestamp)));
            }
        }

        return Task.FromResult<IEnumerable<TagValue>>(allValues);
    }

    private static NodeId GetAggregateNodeId(HdaAggregationType aggregationType)
    {
        return aggregationType switch
        {
            HdaAggregationType.Average => ObjectIds.AggregateFunction_Average,
            HdaAggregationType.Minimum => ObjectIds.AggregateFunction_Minimum,
            HdaAggregationType.Maximum => ObjectIds.AggregateFunction_Maximum,
            HdaAggregationType.Count => ObjectIds.AggregateFunction_Count,
            _ => throw new ArgumentOutOfRangeException(nameof(aggregationType), $"Aggregation type {aggregationType} is not supported.")
        };
    }
}

// --- Placeholder types for compilation ---

/// <summary>
/// Defines the parameters for a historical data query.
/// </summary>
public record HistoricalDataQuery(
    IEnumerable<string> NodeIds,
    DateTime StartTime,
    DateTime EndTime,
    HdaAggregationType AggregationType,
    double? ProcessingInterval = null
);

/// <summary>
/// Defines the type of aggregation to perform on historical data.
/// </summary>
public enum HdaAggregationType
{
    Raw,
    Average,
    Minimum,
    Maximum,
    Count
}