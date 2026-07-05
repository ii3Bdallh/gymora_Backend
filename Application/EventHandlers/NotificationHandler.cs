using Application.DTO;
using Application.Interface.Service;
using Domain.Events;
using MediatR;

namespace Application.EventHandlers;

public class NotificationHandler
    : INotificationHandler<TestEvent>
{
    private readonly INotificationService _notificationService;

    public NotificationHandler(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(
        TestEvent notification,
        CancellationToken cancellationToken)
    {
        //await _notificationService.SendNotificationTestAsync(
        //    new NotificationDTO
        //    {
        //        Title = "Test Notification",
        //        Body = notification.Message
        //    });
    }
}
