using Application.DTO;
using Application.Interface.Service;
using Application.StaticTexts;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class NotificationConsumer :
   IConsumer<PaymentCreatedEvent>
//    , IConsumer<EntityChangedEvent>

{
    private readonly INotificationService _notificationService;

    public NotificationConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var message = context.Message;

        await _notificationService.SendToTopicAsync(
            NotificationTopic.AdminTopic,
            new NotificationDTO
            {
                Title = "Payment Created",
                Body = $"A new payment has been created for request {message.PaymentRequestId}."
            });
    }

    //    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    //    {
    //        var message = context.Message;

    //        await _notificationService.SendNotificationTestAsync(
    //            new NotificationDTO
    //            {
    //                Title = "Welcome",
    //                Body = $"Welcome {message.FullName}"
    //            });
    //    }

    //    public async Task Consume(ConsumeContext<MembershipExpiredEvent> context)
    //    {
    //        var message = context.Message;

    //        await _notificationService.SendNotificationTestAsync(
    //            new NotificationDTO
    //            {
    //                Title = "Membership Expired",
    //                Body = $"Membership of {message.MemberName} has expired."
    //            });
    //    }
}

