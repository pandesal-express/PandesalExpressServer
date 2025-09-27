using StackExchange.Redis;

namespace PandesalExpress.Infrastructure.Services;

public interface ICacheService
{
    // For complete object caching
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);

    // For field-based caching
    Task SetHashFieldAsync(string key, string field, string value, TimeSpan? expiration = null);
    Task SetHashAsync(string key, HashEntry[] fields, TimeSpan? expiration = null);
    Task<string?> GetHashFieldAsync(string key, string field);
    Task<Dictionary<string, string>> GetHashAsync(string key);
    Task RemoveHashFieldAsync(string key, string field);
}
