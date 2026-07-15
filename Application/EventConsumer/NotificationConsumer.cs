using Application.DTO;
using Application.Interface.Service;
using Application.StaticTexts;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumer;

public class NotificationConsumer :
   IConsumer<PaymentCreatedEvent>
   , IConsumer<SubscriptionActivatedEvent>
   , IConsumer<PaymentRejectedEvent>
   , IConsumer<TestNotificationEvent>


{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationConsumer> _logger;

    public NotificationConsumer(INotificationService notificationService, ILogger<NotificationConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("====================================================================");
        _logger.LogInformation("🔔 Sending notification for PaymentCreatedEvent → PaymentRequestId: {PaymentRequestId}", message.PaymentRequestId);
        await _notificationService.SendToTopicAsync(
            NotificationTopic.AdminTopic,
            new NotificationDTO
            {
                Title = "Payment Created",
                Body = $"A new payment has been created for request {message.PaymentRequestId}."
            });
    }

    public async Task Consume(ConsumeContext<SubscriptionActivatedEvent> context)
    {
        var message = context.Message;

        await _notificationService.SendNotificationAsync(
            context.Message.dd,
            new NotificationDTO
            {
                Title = "Subscription Activated",
                Body = $"Your subscription has been activated."
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

    public Task Consume(ConsumeContext<TestNotificationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("====================================================================");
        _logger.LogInformation("🔔 Sending test notification → Message: {Message}", message.Message);
        return _notificationService.SendToTopicAsync(
            NotificationTopic.AdminTopic,
            new NotificationDTO
            {
                Title = "Test Notification",
                Body = message.Message
            });
    }

}

