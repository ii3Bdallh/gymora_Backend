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
using Application.DTO.Exceptions;
using Domain.Enum;

namespace Application.Service
{
    public class PaymentRequestService : BaseAuditableFileService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>, IPaymentRequestService
    {
        private readonly IPaymentRequestRepo _paymentRepo;
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        private readonly ICouponService _couponService;

        private readonly IOwnerSubscriptionRepo _ownerSubscriptionRepo;

        private readonly ICurrentPlanService _currentPlanService;
        public PaymentRequestService(
            IPaymentRequestRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<PaymentRequestService> logger,
            IStorageService storageService,
            ISubscriptionPlanRepo subscriptionPlanRepo,
            ICouponService couponService,
            IOwnerSubscriptionRepo ownerSubscriptionRepo,
            ICurrentPlanService currentPlanService
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _paymentRepo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _couponService = couponService;
            _ownerSubscriptionRepo = ownerSubscriptionRepo;
            _currentPlanService = currentPlanService;
        }
        public override async Task<PaymentRequestRDTO> AddAsync(PaymentRequestCDTO dto, CancellationToken ct = default)
        {
            if (await _paymentRepo.HasPendingRequestAsync(_currentUser.UserId, ct))
                throw new ApplicationException("You already have a pending payment request.");

            var planPrice = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(dto.PlanPriceId, true, false, ct);
            if (planPrice == null)
                throw new ApplicationException("Invalid subscription plan.");


            CurrentPlanResult existingSubscription = await _currentPlanService.GetCurrentPlanAsync(_currentUser.UserId, ct);

            if (existingSubscription.IsFree == false)
                throw new ApplicationException("You already have an active subscription. You cannot create a new payment request until your current subscription expires.");

            decimal discountAmount = 0m;
            int? couponId = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var couponResult = await _couponService.ValidateCouponAsync(
                    dto.CouponCode,
                    planPrice.Amount,
                    planPrice.PlanId,
                    ct);

                if (!couponResult.IsValid)
                    throw new ApplicationException(couponResult.Message);

                if (await _paymentRepo.HasUsedThisCouponBeforeAsync(_currentUser.UserId, couponResult.CouponId!.Value, ct))
                    throw new ApplicationException("You have already used this coupon before.");

                discountAmount = couponResult.DiscountAmount;
                couponId = couponResult.CouponId;


            }

            dto.OriginalAmount = planPrice.Amount;
            dto.DiscountAmount = discountAmount;
            dto.FinalAmount = dto.OriginalAmount - discountAmount;
            dto.CurrencyCode = planPrice.CurrencyCode;
            dto.CouponId = couponId;
            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                // ← هنا نزود UsedCount عند الإنشاء
                if (couponId.HasValue)
                {
                    await _couponService.IncrementUsageAsync(couponId.Value, ct);
                }

                var entity = await base.AddAsync(dto, ct);

                await _publishEndpoint.Publish(new PaymentCreatedEvent(entity.Id), ct);

                await _unitOfWork.CommitTransactionAsync(ct);

                return entity;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }


        }

        public async Task<PaymentRequestRDTO> ApproveAsync(int id, PaymentRequestApprove dto, CancellationToken ct = default)
        {
            var entity = await _paymentRepo.GetByIdAsync(id, true, true, ct);
            if (entity == null)
                throw new NotFoundException("Payment request not found.");

            if (entity.Status != PaymentRequestStatus.Pending)
                throw new ApplicationException("Only pending payment requests can be approved.");

            entity.Status = PaymentRequestStatus.Approved;
            entity.ReviewNotes = dto.ReviewNotes;
            entity.ReviewedBy = _currentUser.UserId;
            entity.ReviewedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            await _publishEndpoint.Publish(new PaymentApprovedEvent(entity.Id, entity.CreatedById, entity.CouponId, entity.DiscountAmount), ct);

            return _mapper.Map<PaymentRequestRDTO>(entity);
        }

        public async Task<PaymentRequestRDTO> RejectAsync(int id, PaymentRequestReject dto, CancellationToken ct = default)
        {
            var entity = await _paymentRepo.GetByIdAsync(id, true, true, ct);
            if (entity == null)
                throw new NotFoundException("Payment request not found.");

            if (entity.Status != PaymentRequestStatus.Pending)
                throw new ApplicationException("Only pending payment requests can be rejected.");

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                entity.Status = PaymentRequestStatus.Rejected;
                entity.RejectionReason = dto.RejectionReason;
                entity.ReviewedBy = _currentUser.UserId;
                entity.ReviewedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(ct);

                // نقص الاستخدام عند الرفض
                if (entity.CouponId.HasValue)
                {
                    await _couponService.DecrementUsageAsync(entity.CouponId.Value, ct);
                }


                await _unitOfWork.CommitTransactionAsync(ct);

                await _publishEndpoint.Publish(new PaymentRejectedEvent(entity.Id, entity.CreatedById, entity.RejectionReason), ct);

                return _mapper.Map<PaymentRequestRDTO>(entity);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;

            }
        }
    }
}