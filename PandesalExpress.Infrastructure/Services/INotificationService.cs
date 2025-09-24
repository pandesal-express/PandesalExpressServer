namespace PandesalExpress.Infrastructure.Services;

public record NotificationDto(
    Guid Id,
    string Message,
    string MessageType, 
    string? Link, 
    DateTime Timestamp,
    bool IsRead = false,
    string? TriggeredBy = null
);

public interface INotificationService
{
    Task SendNotificationToGroupAsync(string groupName, string messageType, NotificationDto payload);
    Task SendNotificationToRolesAsync(List<string> targetRoleNames, string messageType, NotificationDto payload);
}

