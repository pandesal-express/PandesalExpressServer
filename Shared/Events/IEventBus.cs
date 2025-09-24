namespace Shared.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent, 
        CancellationToken cancellationToken = default
    ) where TEvent : IEvent;
}
