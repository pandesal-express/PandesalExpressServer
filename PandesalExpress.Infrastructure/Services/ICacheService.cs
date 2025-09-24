namespace PandesalExpress.Infrastructure.Services;

public interface ICacheService
{
    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    public Task RemoveAsync(string key);
}
