using System.Text.Json;
using Domain.Model;
using Domain.Model.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);

        var entities = context.ChangeTracker
            .Entries<EventEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.Entity.DomainEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    EventType = domainEvent.GetType().FullName!,

                    Payload = JsonSerializer.Serialize(
                        domainEvent,
                        domainEvent.GetType()),

                    OccurredAt = DateTime.UtcNow,

                    CreatedAt = DateTime.UtcNow
                });
            }

            entity.Entity.ClearDomainEvents();
        }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}
