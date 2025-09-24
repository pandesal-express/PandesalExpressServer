using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Services;
using Shared.Events;

namespace PandesalExpress.Host.EventHandlers;

public class TransferRequestEventHandler(
    INotificationService notificationService,
    ILogger<TransferRequestEventHandler> logger
) : IEventHandler<TransferRequestCreatedEvent>, IEventHandler<TransferRequestStatusUpdatedEvent>
{
    public async Task HandleAsync(TransferRequestCreatedEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TransferRequestCreatedEvent for transfer request {TransferId}", 
            integrationEvent.TransferRequest.Id);

        // Create notification for the receiving store
        var receivingStoreId = integrationEvent.TransferRequest.ReceivingStoreId;
        var sendingStoreId = integrationEvent.TransferRequest.SendingStoreId;
        
        var notification = new NotificationDto(
            Guid.NewGuid(),
            $"New transfer request received from store {sendingStoreId}",
            "NewTransferRequest",
            $"/transfers/requests/{integrationEvent.TransferRequest.Id}",
            DateTime.UtcNow,
            false,
            integrationEvent.TransferRequest.InitiatingEmployeeId
        );

        // Send notification to the receiving store group
        await notificationService.SendNotificationToGroupAsync(
            $"Store_{receivingStoreId}", 
            "NewTransferRequest",
            notification
        );
    }

    public async Task HandleAsync(TransferRequestStatusUpdatedEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TransferRequestStatusUpdatedEvent for transfer request {TransferId} with status {Status}", 
            integrationEvent.TransferRequest.Id, integrationEvent.TransferRequest.Status);

        var receivingStoreId = integrationEvent.TransferRequest.ReceivingStoreId;
        var sendingStoreId = integrationEvent.TransferRequest.SendingStoreId;
        
        // Create notification with appropriate message based on status
        string message = integrationEvent.TransferRequest.Status switch
        {
            "Accepted" => "Transfer request has been accepted",
            "Rejected" => "Transfer request has been rejected",
            "Shipped" => "Transfer items have been shipped",
            "Received" => "Transfer items have been received",
            "Cancelled" => "Transfer request has been cancelled",
            _ => $"Transfer request status updated to {integrationEvent.TransferRequest.Status}"
        };

        var notification = new NotificationDto(
            Guid.NewGuid(),
            message,
            "TransferStatusUpdated",
            $"/transfers/requests/{integrationEvent.TransferRequest.Id}",
            DateTime.UtcNow,
            false,
            integrationEvent.TransferRequest.RespondingEmployeeId
        );

        // Determine which store to notify based on the status
        string targetStoreId = integrationEvent.TransferRequest.Status switch
        {
            "Accepted" or "Rejected" => sendingStoreId, // Notify sending store when receiving store responds
            "Shipped" => receivingStoreId, // Notify receiving store when items are shipped
            "Received" => sendingStoreId, // Notify sending store when items are received
            "Cancelled" => integrationEvent.TransferRequest.Status == "Requested" ? receivingStoreId : sendingStoreId,
            _ => receivingStoreId // Default to receiving store
        };

        // Send notification to the appropriate store group
        await notificationService.SendNotificationToGroupAsync(
            $"Store_{targetStoreId}", 
            "TransferStatusUpdated",
            notification
        );
    }
}