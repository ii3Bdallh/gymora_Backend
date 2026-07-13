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
using Application.DTO;
using Domain.Events;

namespace Application.Service
{
    public class PaymentRequestService : BaseAuditableFileService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>, IPaymentRequestService
    {
        private readonly IPaymentRequestRepo _paymentRepo;
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        public PaymentRequestService(
            IPaymentRequestRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<PaymentRequestService> logger,
            IStorageService storageService,
            ISubscriptionPlanRepo subscriptionPlanRepo
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _paymentRepo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
        }
        public override async Task<PaymentRequestRDTO> AddAsync(PaymentRequestCDTO dto, CancellationToken ct = default)
        {
            if (await _paymentRepo.HasPendingRequestAsync(_currentUser.UserId, ct))
                throw new ApplicationException("You already have a pending payment request. Please wait for it to be reviewed before submitting a new one.");

            var planPrice = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(dto.PlanPriceId, true, false, ct);

            if (planPrice == null || planPrice.PlanId != dto.PlanId)
                throw new ApplicationException("Invalid subscription plan.");

            dto.OriginalAmount = planPrice.Amount;
            dto.DiscountAmount = 0;
            dto.FinalAmount = dto.OriginalAmount - dto.DiscountAmount;
            dto.CurrencyCode = planPrice.CurrencyCode;
            PaymentRequestRDTO entity = await base.AddAsync(dto, ct);

            await _publishEndpoint.Publish(new PaymentCreatedEvent(entity.Id), ct);
            return entity;
        }
    }
}