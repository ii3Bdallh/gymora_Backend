// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Application.Interface.Repo;
// using Domain.Events;
// using MassTransit;

// namespace Application.EventConsumer
// {
//     public class OwnerSubscriptionConsumer : IConsumer<SubscriptionActivatedEvent>
//     {

//         IPaymentRequestRepo _paymentRequestRepo;
//         public OwnerSubscriptionConsumer(IPaymentRequestRepo paymentRequestRepo)
//         {
//             _paymentRequestRepo = paymentRequestRepo;
//         }
//         public async Task Consume(ConsumeContext<SubscriptionActivatedEvent> context)
//         {
//             int subscriptionId = context.Message.SubscriptionId;
//             int paymentRequestId = context.Message.PaymentRequestId;

//             var affectd = await _paymentRequestRepo.ConnectPaymentRequestToSubscriptionAsync(paymentRequestId, subscriptionId, context.CancellationToken);

//             if (affectd == 0)
//             {
//                 throw new ApplicationException($"Failed to connect PaymentRequest {paymentRequestId} to Subscription {subscriptionId}");
//             }
//         }


//     }
// }