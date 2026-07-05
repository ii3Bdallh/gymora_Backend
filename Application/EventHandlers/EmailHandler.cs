using Application.Interface.Service.Shared;
using Domain.Events;
using MediatR;

namespace Application.EventHandlers;

public class EmailHandler
    : INotificationHandler<TestEvent>
{
    private readonly IEmailService _emailService;

    public EmailHandler(
        IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        TestEvent notification,
        CancellationToken cancellationToken)
    {
        await _emailService.SendEmailTestAsync(
            notification.Email,
            "Test Email",
            notification.Message);
    }
}
