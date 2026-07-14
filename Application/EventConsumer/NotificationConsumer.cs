using Application.DTO;
using Application.Interface.Service;
using Application.StaticTexts;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class NotificationConsumer :
   IConsumer<PaymentCreatedEvent>
   , IConsumer<PaymentApprovedEvent>
   , IConsumer<PaymentRejectedEvent>
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

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;

        await _notificationService.SendNotificationAsync(
            context.Message.UserId,
            new NotificationDTO
            {
                Title = "Payment Approved",
                Body = $"Payment request {message.PaymentRequestId} has been approved \n You can now enjoy your subscription."
            });
    }

    public async Task Consume(ConsumeContext<PaymentRejectedEvent> context)
    {
        var message = context.Message;

        await _notificationService.SendNotificationAsync(
               context.Message.UserId,
            new NotificationDTO
            {
                Title = "Payment Rejected",
                Body = $"Payment request {message.PaymentRequestId} has been rejected."
            });
    }

 
}

