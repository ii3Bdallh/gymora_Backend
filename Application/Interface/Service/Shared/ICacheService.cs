namespace Application.Interface.Service.Shared;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);

    Task RemoveAsync(string key);

    Task RemoveByPrefixAsync(string prefix);

    Task InvalidateEntityAsync(
    string entityName,
    int entityId,
    int? gymId,
    int? userId);
}
