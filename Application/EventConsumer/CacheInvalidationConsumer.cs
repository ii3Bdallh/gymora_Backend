using Application.Cache;
using Application.Interface.Service.Shared;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class CacheInvalidationConsumer : IConsumer<EntityChangedEvent>
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationConsumer(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<EntityChangedEvent> context)
    {
        var e = context.Message;
        var entityName = e.EntityName.ToLower();

        // Remove single entity
        await _cacheService.RemoveAsync(CacheKeyGenerator.ById(entityName, e.EntityId, e.GymId));

        // Remove all pages and lists for this entity (both gym-scoped and global)
        if (e.GymId.HasValue)
        {
            await _cacheService.RemoveByPrefixAsync($"{CacheKeyGenerator.GymPrefix(e.GymId.Value)}:{entityName}:");
        }
        else
        {
            await _cacheService.RemoveByPrefixAsync($"{CacheKeyGenerator.GlobalPrefix()}:{entityName}:");
        }
    }
}
