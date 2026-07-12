using Application.Cache;
using Application.Interface.Service.Shared;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumer;
public class CacheInvalidationConsumer : IConsumer<EntityChangedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationConsumer> _logger;   // ← أضف ده

    public CacheInvalidationConsumer(ICacheService cacheService, ILogger<CacheInvalidationConsumer> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntityChangedEvent> context)
    {
        var e = context.Message;
        var entityName = e.EntityName.ToLower();

        _logger.LogInformation("🔄 Cache Invalidation Started → Entity: {EntityName}, ID: {Id}, GymId: {GymId}", 
            entityName, e.EntityId, e.GymId);

        // Remove single entity
        await _cacheService.RemoveAsync(CacheKeyGenerator.ById(entityName, e.EntityId, e.GymId));

        // Remove pages
        if (e.GymId.HasValue)
        {
            await _cacheService.RemoveByPrefixAsync($"{CacheKeyGenerator.GymPrefix(e.GymId.Value)}:{entityName}:");
            _logger.LogDebug("Cleared gym-scoped cache for {Entity}", entityName);
        }
        else
        {
            await _cacheService.RemoveByPrefixAsync($"{CacheKeyGenerator.GlobalPrefix()}:{entityName}:");
            _logger.LogDebug("Cleared global cache for {Entity}", entityName);
        }

        _logger.LogInformation("✅ Cache Invalidation Completed for {Entity} ID {Id}", entityName, e.EntityId);
    }



}

