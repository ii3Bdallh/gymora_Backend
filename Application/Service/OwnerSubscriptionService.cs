using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Domain.Enum;
using Domain.Events;
using Application.DTO.Exceptions;

namespace Application.Service
{
    public class OwnerSubscriptionService : BaseAuditableService<OwnerSubscription, OwnerSubscriptionRDTO, OwnerSubscriptionCDTO, OwnerSubscriptionUDTO>, IOwnerSubscriptionService
    {
        private readonly IPaymentRequestRepo _paymentRequestRepo;
        private readonly IOwnerSubscriptionRepo _ownerSubscriptionRepo;

        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        public OwnerSubscriptionService(
            IOwnerSubscriptionRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<OwnerSubscriptionService> logger,
            IPaymentRequestRepo paymentRequestRepo,
            ISubscriptionPlanRepo subscriptionPlanRepo
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _paymentRequestRepo = paymentRequestRepo;
            _ownerSubscriptionRepo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
        }
        public async Task<OwnerSubscriptionRDTO> CreateFromApprovedPaymentAsync(int paymentRequestId, CancellationToken ct = default)
        {
            var payment = await _paymentRequestRepo.GetByIdIgnoringSecurityAsync(paymentRequestId, true, false, ct);
            if (payment == null || payment.Status != PaymentRequestStatus.Approved)
                throw new ApplicationException("Payment request is not approved.");

            // التحقق إن مفيش اشتراك نشط
            if (await _ownerSubscriptionRepo.HasActiveSubscriptionAsync(payment.CreatedById, ct))
                throw new ApplicationException("User already has an active subscription.");

            PlanPrice? planPrice = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(payment.PlanPriceId, true, false, ct);
            if (planPrice == null)
                throw new ApplicationException("Invalid subscription plan price.");

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths(planPrice.DurationMonths);

            var subscription = new OwnerSubscription
            {
                CreatedById = payment.CreatedById,
                PlanId = payment.PlanId,
                PlanPriceId = payment.PlanPriceId,
                PaymentRequestId = payment.Id,
                AmountPaid = payment.FinalAmount,
                CurrencyCode = payment.CurrencyCode,
                StartDate = startDate,
                EndDate = endDate,
                GraceEndDate = endDate.AddDays(7)

            };

            await _repo.AddAsync(subscription, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            var result = _mapper.Map<OwnerSubscriptionRDTO>(subscription);

            // نشر Event
            await _publishEndpoint.Publish(new SubscriptionActivatedEvent(result.Id, payment.Id, payment.CreatedById), ct);

            return result;
        }

        public async Task<OwnerSubscriptionRDTO> GetCurrentSubscriptionAsync(int ownerUserId, CancellationToken ct = default)
        {

            var subscription = await _ownerSubscriptionRepo.GetCurrentSubscriptionAsync(ownerUserId, ct);
            if (subscription == null || !CanModify(subscription))
            {
                _logger.LogWarning("Unauthorized or failed attempt to access subscription for user with ID {UserId} by user {CurrentUserId}", ownerUserId, CurrentUserId);
                throw new NotFoundException($"Subscription for user with ID {ownerUserId} was not found.");
            }
            return _mapper.Map<OwnerSubscriptionRDTO>(subscription);
        }
    }
}
