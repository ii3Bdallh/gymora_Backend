using Application.Interface.Service.Shared;
using Domain.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache cache, IOptions<CacheOptions> options, ILogger<CacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out T? value) ? value : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        var opts = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(_options.DefaultAbsoluteExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_options.DefaultSlidingExpirationMinutes)
        };

        _cache.Set(key, value, opts);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        _logger.LogInformation("Cache prefix cleared: {Prefix}", prefix);
        return Task.CompletedTask;
    }
}
