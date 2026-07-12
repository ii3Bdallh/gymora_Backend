//using Application.DTO;
//using Application.Interface.Service;
//using Domain.Events;
//using MassTransit;

//namespace Application.EventConsumer;

//public class NotificationConsumer :
//    IConsumer<ChildAddedEvent>,
//    IConsumer<UserRegisteredEvent>,
//    IConsumer<MembershipExpiredEvent>
//{
//    private readonly INotificationService _notificationService;

//    public NotificationConsumer(INotificationService notificationService)
//    {
//        _notificationService = notificationService;
//    }

//    public async Task Consume(ConsumeContext<ChildAddedEvent> context)
//    {
//        var message = context.Message;

//        await _notificationService.SendNotificationTestAsync(
//            new NotificationDTO
//            {
//                Title = "Child Added",
//                Body = message.ChildName
//            });
//    }

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
//}