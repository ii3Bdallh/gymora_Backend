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

    public async Task InvalidateEntityAsync(
    string entityName,
    int entityId,
    int? gymId,
    int? userId)
    {
        entityName = entityName.ToLower();

        await RemoveAsync(
            CacheKeyGenerator.ById(
                entityName,
                entityId,
                gymId,
                userId));

        if (gymId.HasValue)
        {
            await RemoveByPrefixAsync(
                $"{CacheKeyGenerator.GymPrefix(gymId.Value)}:{entityName}:");
        }
        else
        {
            await RemoveByPrefixAsync(
                $"{CacheKeyGenerator.GlobalPrefix()}:{entityName}:");
        }

        _logger.LogInformation(
            "Cache invalidated for {Entity}",
            entityName);
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return Task.CompletedTask;

        // فلترة المفاتيح التي تبدأ بالـ Prefix المطلوب
        var keysToRemove = CacheKeys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            CacheKeys.TryRemove(key, out _);
        }

        _logger.LogInformation("Cache prefix cleared: {Prefix}. Removed {Count} keys.", prefix, keysToRemove.Count);
        return Task.CompletedTask;
    }
}