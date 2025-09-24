using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PandesalExpress.Infrastructure.Services;
using StackExchange.Redis;

namespace PandesalExpress.Host.Hubs;

[Authorize]
public class NotificationHub(
    IConnectionMultiplexer redis,
    ILogger<NotificationHub> logger
) : Hub
{
    private const string UserLastSeenHashKey = "user:last_seen_notification_timestamp";
    private const string GlobalNotificationsSortedSetKey = "notifications:all";
    
    public override async Task OnConnectedAsync()
    {
        ClaimsPrincipal? user = Context.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user != null && !string.IsNullOrEmpty(userId))
        {
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userRoles.Count != 0)
            {
                foreach (var roleName in userRoles.Distinct())
                    await Groups.AddToGroupAsync(Context.ConnectionId, roleName);
            }
            
            await SendMissedNotificationsToCaller(userId);
        }
        
        await base.OnConnectedAsync();
    }

    private async Task SendMissedNotificationsToCaller(string userId)
    {
        IDatabase db = redis.GetDatabase();

        try
        {
            // get the user's last seen timestamp from the Hash
            RedisValue lastSeenValue = await db.HashGetAsync(UserLastSeenHashKey, userId);
            long lastSeenTimestamp = lastSeenValue.HasValue ? (long)lastSeenValue : 0;

            // query the Sorted Set for all notifications with a score (timestamp) greater than the last seen one
            RedisValue[] redisValues = await db.SortedSetRangeByScoreAsync(
                GlobalNotificationsSortedSetKey,
                start: lastSeenTimestamp,
                stop: double.PositiveInfinity,
                exclude: Exclude.Start
            );
            
            if (redisValues.Length != 0)
            {
                var notifications = redisValues
                    .Select(val => JsonSerializer.Deserialize<NotificationDto>(val!))
                    .Where(n => n != null).ToList();

                if (notifications.Count != 0)
                {
                    await Clients.Caller.SendAsync("ReceiveMissedNotifications", notifications);

                    // update the user's "last seen" timestamp to the newest notification sent
                    var latestTimestamp = notifications.Max(n => n!.Timestamp.Ticks);
                    await db.HashSetAsync(UserLastSeenHashKey, userId, latestTimestamp);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching missed notifications from Redis for user {UserId}", userId);
        }
    }

    // This will be called by the client after they display notifications to confirm receipt
    public async Task AcknowledgeNotifications(long latestTimestampTicks)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;
        
        IDatabase db = redis.GetDatabase();
        await db.HashSetAsync(UserLastSeenHashKey, userId, latestTimestampTicks);
        
        logger.LogInformation("User {UserId} acknowledged notifications up to timestamp {Timestamp}", userId, latestTimestampTicks);
    }
}

