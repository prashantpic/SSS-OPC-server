using Opc.System.Shared.Kernel.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Shared.Kernel.Infrastructure.Abstractions;

/// <summary>
/// Defines the contract for an event bus to enable event-driven communication between services.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the message bus.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to an integration event of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the integration event.</typeparam>
    /// <typeparam name="TH">The type of the event handler.</typeparam>
    /// <param name="cancellationToken">A token to cancel the subscription setup.</param>
    Task SubscribeAsync<T, TH>(CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>;
}

/// <summary>
/// Base handler interface for integration events.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Handles the received integration event.
    /// </summary>
    /// <param name="event">The integration event to handle.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task Handle(TIntegrationEvent @event);
}