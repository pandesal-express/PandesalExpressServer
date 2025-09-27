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

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (Equals(value, default(T)))
        {
            logger.LogWarning("Attempted to cache null value for key: {CacheKey}", key);
            return;
        }

        string serializedData = JsonSerializer.Serialize(value);
        TimeSpan expiry = expiration ?? TimeSpan.FromMinutes(10);

        bool success = await _db.StringSetAsync(key, serializedData, expiry);

        if (success)
            logger.LogInformation("Cache SET for key: {CacheKey}", key);
        else
            logger.LogError("Failed to set cache for key: {CacheKey}", key);
    }


    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
        logger.LogInformation("Cache INVALIDATED for key: {CacheKey}", key);
    }

    public async Task SetHashFieldAsync(string key, string field, string value, TimeSpan? expiration = null)
    {
        await _db.HashSetAsync(key, field, value);

        if (expiration.HasValue)
            await _db.KeyExpireAsync(key, expiration.Value);

        logger.LogInformation("Cache HASH SET for key: {CacheKey}, field: {Field}", key, field);
    }

    public async Task SetHashAsync(string key, HashEntry[] fields, TimeSpan? expiration = null)
    {
        await _db.HashSetAsync(key, fields);

        if (expiration.HasValue) await _db.KeyExpireAsync(key, expiration.Value);

        logger.LogInformation("Cache HASH SET for key: {CacheKey} with {FieldCount} fields", key, fields.Length);
    }

    public async Task<string?> GetHashFieldAsync(string key, string field)
    {
        RedisValue value = await _db.HashGetAsync(key, field);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<Dictionary<string, string>> GetHashAsync(string key)
    {
        HashEntry[] entries = await _db.HashGetAllAsync(key);
        return entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());
    }

    public async Task RemoveHashFieldAsync(string key, string field)
    {
        await _db.HashDeleteAsync(key, field);
        logger.LogInformation("Cache HASH field removed for key: {CacheKey}, field: {Field}", key, field);
    }
}
