using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Events;

public class InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        IEnumerable<IEventHandler<TEvent>> handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (IEventHandler<TEvent> handler in handlers)
        {
            try { await handler.HandleAsync(integrationEvent, cancellationToken); }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error handling event {EventName} with handler {HandlerName}",
                    typeof(TEvent).Name,
                    handler.GetType().Name
                );
                
                break;
            }
        }
    }
}

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}
