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
        logger.LogInformation(
            "Handling TransferRequestCreatedEvent for transfer request {TransferId}",
            integrationEvent.TransferRequest.Id
        );

        string? receivingStoreId = integrationEvent.TransferRequest.ReceivingStoreId;
        string? sendingStoreId = integrationEvent.TransferRequest.SendingStoreId;

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

        // TODO: Send a real-time data to both receiving and sending stores for creating a request transfer
    }

    public async Task HandleAsync(TransferRequestStatusUpdatedEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling TransferRequestStatusUpdatedEvent for transfer request {TransferId} with status {Status}",
            integrationEvent.TransferRequest.Id,
            integrationEvent.TransferRequest.Status
        );

        string? receivingStoreId = integrationEvent.TransferRequest.ReceivingStoreId;
        string? sendingStoreId = integrationEvent.TransferRequest.SendingStoreId;

        string message = integrationEvent.TransferRequest.Status switch
        {
            "Accepted" => "Transfer request has been accepted",
            "Rejected" => "Transfer request has been rejected",
            "Shipped" => "Transfer items have been shipped",
            "Received" => "Transfer items have been received",
            "Cancelled" => "Transfer request has been cancelled",
            var _ => $"Transfer request status updated to {integrationEvent.TransferRequest.Status}"
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
        string? targetStoreId = integrationEvent.TransferRequest.Status switch
        {
            "Accepted" or "Rejected" => sendingStoreId,
            "Shipped" => receivingStoreId,
            "Received" => sendingStoreId,
            "Cancelled" => integrationEvent.TransferRequest.Status == "Requested" ? receivingStoreId : sendingStoreId,
            var _ => receivingStoreId
        };

        await notificationService.SendNotificationToGroupAsync(
            $"Store_{targetStoreId}",
            "TransferStatusUpdated",
            notification
        );

        // TODO: Send data to both receiving and sending stores for transfer updates
    }
}
