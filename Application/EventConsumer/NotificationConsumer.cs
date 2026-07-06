using Application.DTO;
using Application.Interface.Service;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class NotificationConsumer
    : IConsumer<ChildAddedEvent>
{
    private readonly INotificationService _notificationService;

    public NotificationConsumer(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(
        ConsumeContext<ChildAddedEvent> context)
    {
        var notification = context.Message;
        
        await _notificationService.SendNotificationTestAsync(
           new NotificationDTO
           {
               Title = "Test Notification",
               Body = notification.ChildName
           });
    }
}
