using Application.Interface.Service.Shared;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class EmailConsumer :
   IConsumer<PaymentCreatedEvent>
//    ,IConsumer<UserRegisteredEvent>
//    ,IConsumer<PasswordResetEvent>
{
    private readonly IEmailService _emailService;

    public EmailConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {

        await _emailService.SendEmailAsync(
            "Abdallhmamdouh079@gmail.com",
            "Payment Created",
      $"A new payment has been created for request {context.Message.PaymentRequestId}.");
    }

    //    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    //    {
    //        var message = context.Message;

    //        await _emailService.SendWelcomeEmailAsync(
    //            message.Email,
    //            message.FullName);
    //    }

    //    public async Task Consume(ConsumeContext<PasswordResetEvent> context)
    //    {
    //        var message = context.Message;

    //        await _emailService.SendPasswordResetEmailAsync(
    //            message.Email,
    //            message.ResetLink);
    //    }
}