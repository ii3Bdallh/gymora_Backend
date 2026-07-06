using Application.Interface.Service.Shared;
using Domain.Events;
using  MassTransit;

namespace Application.EventConsumer;

public class EmailConsumer
    : IConsumer<ChildAddedEvent>
{
    private readonly IEmailService _emailService;

    public EmailConsumer(
        IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(
        ConsumeContext<ChildAddedEvent> context)
    {
        var notification = context.Message;

        await _emailService.SendEmailTestAsync(
            notification.ParentEmail,
            "Test Email",
            notification.ChildName);
    }
}
