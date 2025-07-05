using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories;

/// <summary>
/// Defines the contract for a factory that creates or resolves the correct
/// data lifecycle strategy based on a data type. This decouples the job
/// from concrete strategy implementations, enabling easy registration and retrieval.
/// </summary>
public interface IDataLifecycleStrategyFactory
{
    /// <summary>
    /// Gets the appropriate IDataLifecycleStrategy implementation for a given data type.
    /// </summary>
    /// <param name="dataType">The data type for which to find a strategy.</param>
    /// <returns>The corresponding data lifecycle strategy.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if no strategy is registered for the specified data type.
    /// </exception>
    IDataLifecycleStrategy GetStrategy(DataType dataType);
}