using Microsoft.AspNetCore.SignalR;
using PandesalExpress.Host.Hubs;
using Shared.Dtos;
using Shared.Events;

namespace PandesalExpress.Host.EventHandlers;

public class PdndRequestEventHandler(
    IHubContext<NotificationHub> hubContext, 
    ILogger<PdndRequestEventHandler> logger
) : IEventHandler<PdndRequestEvent>
{
    public async Task HandleAsync(PdndRequestEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Debug
        logger.LogInformation("Received PdndRequestEvent: {Event}", integrationEvent);

        // The payload
        PdndRequestDto notificationDto = integrationEvent.PdndRequest;

        await hubContext.Clients.Group("Commissary").SendAsync("NewPdndRequest", notificationDto, cancellationToken);
    }
}
