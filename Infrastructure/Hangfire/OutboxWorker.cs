using System.Text.Json;
using Domain.Events;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Hangfire;

public class OutboxWorker
{
    private readonly ApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public OutboxWorker(
        ApplicationDbContext context,
        IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task ProcessOutboxMessagesAsync()
    {
        var messages = await _context.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                if (message.EventType ==
                    typeof(TestEvent).FullName)
                {
                    var testEvent =
                        JsonSerializer.Deserialize<TestEvent>(
                            message.Payload)!;

                    await _publisher.Publish(testEvent);
                }

                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;

                message.ErrorMessage = ex.Message;
            }
        }

        await _context.SaveChangesAsync();
    }
}
