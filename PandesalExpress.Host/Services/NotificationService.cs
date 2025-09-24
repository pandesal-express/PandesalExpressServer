using Microsoft.AspNetCore.SignalR;
using PandesalExpress.Host.Hubs;
using PandesalExpress.Infrastructure.Services;
using StackExchange.Redis; 
using System.Text.Json;

namespace PandesalExpress.Host.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    IConnectionMultiplexer redis,
    ILogger<NotificationService> logger
) : INotificationService
{
    public async Task SendNotificationToGroupAsync(string groupName, string messageType, NotificationDto payload)
    {
        logger.LogInformation("Pushing SignalR message '{MessageType}' to group '{GroupName}'.", messageType, groupName);
        await hubContext.Clients.Group(groupName).SendAsync(messageType, payload);
        
        IDatabase db = redis.GetDatabase();
        const string redisKey = "notifications:all";

        try
        {
            var serializedPayload = JsonSerializer.Serialize(payload);
            var timestampScore = payload.Timestamp.Ticks;

            await db.SortedSetAddAsync(redisKey, serializedPayload, timestampScore);
            
            var thirtyDaysAgoTicks = DateTime.UtcNow.AddDays(-30).Ticks;
            await db.SortedSetRemoveRangeByScoreAsync(redisKey, 0, thirtyDaysAgoTicks);

            logger.LogInformation("Persisted notification {NotificationId} to Redis sorted set '{RedisKey}'.", payload.Id, redisKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist notification to Redis");
        }
    }

    public async Task SendNotificationToRolesAsync(List<string> targetRoleNames, string messageType, NotificationDto payload)
    {
        if (targetRoleNames.Count == 0) return;
        
        await hubContext.Clients.Groups(targetRoleNames).SendAsync(messageType, payload);
        
        IDatabase db = redis.GetDatabase();
        const string redisKey = "notifications:all";
        var serializedPayload = JsonSerializer.Serialize(payload);
        var timestampScore = payload.Timestamp.Ticks;

        try
        {
            await db.SortedSetAddAsync(redisKey, serializedPayload, timestampScore);

            var thirtyDaysAgoTicks = DateTime.UtcNow.AddDays(-30).Ticks;
            await db.SortedSetRemoveRangeByScoreAsync(redisKey, 0, thirtyDaysAgoTicks);
            
            logger.LogInformation("Persisted notification {NotificationId} to global Redis sorted set.", payload.Id);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to persist notification to Redis sorted set."); }
    }
}