using Application.Cache;
using Application.Interface.Service.Shared;
using Domain.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<CacheService> _logger;

    // قاموس لتخزين جميع الـ Keys النشطة في الذاكرة بشكل آمن (Thread-Safe)
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

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

        // عند انتهاء صلاحية الكاش تلقائياً، نقوم بحذف المفتاح من القاموس الخاص بنا
        opts.RegisterPostEvictionCallback((evictedKey, value, reason, state) =>
        {
            CacheKeys.TryRemove(evictedKey.ToString()!, out _);
        });

        _cache.Set(key, value, opts);
        CacheKeys.TryAdd(key, true); // إضافة المفتاح للقاموس

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        CacheKeys.TryRemove(key, out _); // حذف المفتاح من القاموس
        return Task.CompletedTask;
    }



}