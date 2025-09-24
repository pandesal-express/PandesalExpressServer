using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace PandesalExpress.Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        RedisValue cachedValue = await _db.StringGetAsync(key);

        if (!cachedValue.IsNullOrEmpty)
        {
            logger.LogInformation("Cache HIT for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedValue!);
        }

        logger.LogInformation("Cache MISS for key: {CacheKey}. Fetching from data source.", key);

        T data = await factory();
        string serializedData = JsonSerializer.Serialize(data);
        TimeSpan expiry = expiration ?? TimeSpan.FromMinutes(10);

        await _db.StringSetAsync(key, serializedData, expiry);

        return data;
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
        logger.LogInformation("Cache INVALIDATED for key: {CacheKey}", key);
    }
}
