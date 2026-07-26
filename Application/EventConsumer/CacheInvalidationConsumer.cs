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
        string key = CacheKeyGenerator.ById(
            e.EntityName,
            e.EntityId,
            e.GymId,
            e.UserId);

        await _cacheService.RemoveAsync(key);
    }

}

