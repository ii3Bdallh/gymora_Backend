using Application.Interface.Service.Shared;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer;

public class EmailConsumer :
   IConsumer<PaymentCreatedEvent>
   , IConsumer<PaymentRejectedEvent>
//    , IConsumer<InvitationCreatedEvent>
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

    public async Task Consume(ConsumeContext<PaymentRejectedEvent> context)
    {
        await _emailService.SendEmailAsync(
        "Abdallhmamdouh079@gmail.com",
        "Payment Rejected",
        $"Your payment request {context.Message.PaymentRequestId} has been rejected.");
    }

    // public async Task Consume(ConsumeContext<InvitationCreatedEvent> context)
    // {
    //     var message = context.Message;

    //     await _emailService.SendEmailAsync(
    //         message.Email,
    //         "You're Invited to Join a Gym on Gymora! 🎉",
    //         $"Hello!\n\n" +
    //         $"You have been invited to join the gym as a {message.GymRole}.\n\n" +
    //         $"Open the Gymora app and check your invitations to accept or decline.\n\n" +
    //         $"— Gymora Team");
    // }
}