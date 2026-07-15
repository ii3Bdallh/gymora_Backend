using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Domain.Events;
using Domain.Model;
using MassTransit;

namespace Application.EventConsumer
{
    public class PaymentRequestApprovedConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly IOwnerSubscriptionService _subscriptionService;
        private readonly ICouponRedemptionRepo _couponRedemptionRepo;

        private readonly IUnitOfWork _unitOfWork;
        public PaymentRequestApprovedConsumer(IOwnerSubscriptionService subscriptionService, ICouponRedemptionRepo couponRedemptionRepo, IUnitOfWork unitOfWork)
        {
            _subscriptionService = subscriptionService;
            _couponRedemptionRepo = couponRedemptionRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            int paymentRequestId = context.Message.PaymentRequestId;
            int userId = context.Message.UserId;
            int? couponId = context.Message.CouponId;
            decimal? discountAmount = context.Message.DiscountAmount;
            await _subscriptionService.CreateFromApprovedPaymentAsync(paymentRequestId, context.CancellationToken);

            if (couponId.HasValue && discountAmount.HasValue)
            {
                var couponRedemption = new CouponRedemption
                {
                    CouponId = couponId.Value,
                    PaymentRequestId = paymentRequestId,
                    DiscountAmount = discountAmount.Value,
                    CreatedById = userId,
                    
                };
                await _couponRedemptionRepo.AddAsync(couponRedemption, context.CancellationToken);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            }
            return;
        }
    }
}