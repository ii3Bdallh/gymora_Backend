using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interface.Service;
using Domain.Events;
using MassTransit;

namespace Application.EventConsumer
{
    public class PaymentRequestApprovedConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly IOwnerSubscriptionService _subscriptionService;
        public PaymentRequestApprovedConsumer(IOwnerSubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            int paymentRequestId = context.Message.PaymentRequestId;
            await _subscriptionService.CreateFromApprovedPaymentAsync(paymentRequestId, context.CancellationToken);
            return;
        }
    }
}