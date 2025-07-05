using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;
using System.Collections.ObjectModel;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories;

/// <summary>
/// Implements the strategy factory. It holds a collection of all registered
/// IDataLifecycleStrategy instances and returns the correct one on request.
/// </summary>
public sealed class DataLifecycleStrategyFactory : IDataLifecycleStrategyFactory
{
    private readonly IReadOnlyDictionary<DataType, IDataLifecycleStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataLifecycleStrategyFactory"/> class.
    /// </summary>
    /// <param name="strategies">
    /// A collection of all IDataLifecycleStrategy implementations, injected by the DI container.
    /// </param>
    public DataLifecycleStrategyFactory(IEnumerable<IDataLifecycleStrategy> strategies)
    {
        // The constructor populates a dictionary by mapping each strategy's DataType property
        // to the strategy instance itself. This provides fast O(1) lookups.
        _strategies = strategies.ToDictionary(s => s.DataType);
    }

    /// <inheritdoc/>
    public IDataLifecycleStrategy GetStrategy(DataType dataType)
    {
        if (_strategies.TryGetValue(dataType, out var strategy))
        {
            return strategy;
        }

        throw new NotSupportedException($"Data lifecycle strategy for data type '{dataType}' is not registered.");
    }
}