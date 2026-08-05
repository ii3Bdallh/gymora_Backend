using Application.Cache;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service;
using Application.Service.Entity;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.Helpers;

public static class Mocks
{
    public static Mock<ICouponRepo> CouponRepo() => new();
    public static Mock<ICouponRedemptionRepo> CouponRedemptionRepo() => new();
    public static Mock<IOwnerSubscriptionRepo> OwnerSubscriptionRepo() => new();
    public static Mock<IPaymentRequestRepo> PaymentRequestRepo() => new();
    public static Mock<ISubscriptionPlanRepo> SubscriptionPlanRepo() => new();
    public static Mock<IUnitOfWork> UnitOfWork() => new();
    public static Mock<IMapper> Mapper() => new();
    public static Mock<ICacheService> CacheService() => new();
    public static Mock<IPublishEndpoint> PublishEndpoint() => new();
    public static Mock<IStorageService> StorageService() => new();
    public static Mock<ICurrentPlanService> CurrentPlanService() => new();
    public static Mock<IAuthService> AuthService() => new();
    public static Mock<ICouponService> CouponService() => new();
    public static Mock<ILogger<CouponService>> CouponLogger() => new();
    public static Mock<ILogger<CouponRedemptionService>> CouponRedemptionLogger() => new();
    public static Mock<ILogger<OwnerSubscriptionService>> OwnerSubscriptionLogger() => new();
    public static Mock<ILogger<PaymentRequestService>> PaymentRequestLogger() => new();
    public static Mock<ILogger<SubscriptionPlanService>> SubscriptionPlanLogger() => new();

    public static CurrentUser DefaultCurrentUser(int userId = 1, int? gymId = null, string? platformRole = null)
    {
        return new CurrentUser
        {
            UserId = userId,
            CurrentGymId = gymId,
            PlatformRole = platformRole,
            IsAuthenticated = true
        };
    }

    public static CurrentUser SuperAdminCurrentUser(int userId = 1)
    {
        return new CurrentUser
        {
            UserId = userId,
            PlatformRole = "SuperAdmin",
            IsAuthenticated = true
        };
    }
}

public static class TestData
{
    public static Coupon CreateCoupon(
        int id = 1,
        string code = "TEST10",
        string name = "Test Coupon",
        decimal discountValue = 10m,
        DiscountType discountType = DiscountType.FixedAmount,
        int? usageLimit = 100,
        int usedCount = 0,
        bool isActive = true,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        decimal? maxDiscountAmount = null,
        decimal? minimumPurchaseAmount = null)
    {
        return new Coupon
        {
            Id = id,
            Code = code,
            Name = name,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxDiscountAmount = maxDiscountAmount,
            MinimumPurchaseAmount = minimumPurchaseAmount,
            UsageLimit = usageLimit,
            UsedCount = usedCount,
            ValidFrom = validFrom ?? DateTime.UtcNow.AddDays(-1),
            ValidTo = validTo ?? DateTime.UtcNow.AddDays(30),
            RowVersion = [1, 2, 3]
        };
    }

    public static CouponRedemption CreateCouponRedemption(
        int id = 1,
        int couponId = 1,
        int paymentRequestId = 1,
        decimal discountAmount = 10m,
        int createdById = 1)
    {
        return new CouponRedemption
        {
            Id = id,
            CouponId = couponId,
            PaymentRequestId = paymentRequestId,
            DiscountAmount = discountAmount,
            CreatedById = createdById,
            CreatedOn = DateTime.UtcNow
        };
    }

    public static OwnerSubscription CreateOwnerSubscription(
        int id = 1,
        int planId = 1,
        int planPriceId = 1,
        int? paymentRequestId = 1,
        decimal amountPaid = 50m,
        string currencyCode = "USD",
        int createdById = 1,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DateTime? graceEndDate = null)
    {
        var start = startDate ?? DateTime.UtcNow;
        var end = endDate ?? start.AddMonths(1);
        var grace = graceEndDate ?? end.AddDays(7);
        return new OwnerSubscription
        {
            Id = id,
            PlanId = planId,
            PlanPriceId = planPriceId,
            PaymentRequestId = paymentRequestId,
            AmountPaid = amountPaid,
            CurrencyCode = currencyCode,
            CreatedById = createdById,
            CreatedOn = DateTime.UtcNow,
            StartDate = start,
            EndDate = end,
            GraceEndDate = grace,
            RowVersion = [1, 2, 3]
        };
    }

    public static PaymentRequest CreatePaymentRequest(
        int id = 1,
        int planId = 1,
        int planPriceId = 1,
        int? couponId = null,
        string? couponCode = null,
        decimal originalAmount = 100m,
        decimal discountAmount = 0m,
        decimal finalAmount = 100m,
        string currencyCode = "USD",
        PaymentRequestStatus status = PaymentRequestStatus.Pending,
        int createdById = 1)
    {
        return new PaymentRequest
        {
            Id = id,
            PlanId = planId,
            PlanPriceId = planPriceId,
            CouponId = couponId,
            CouponCode = couponCode,
            OriginalAmount = originalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            CurrencyCode = currencyCode,
            Status = status,
            CreatedById = createdById,
            CreatedOn = DateTime.UtcNow,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
    }

    public static SubscriptionPlan CreateSubscriptionPlan(
        int id = 1,
        string name = "Basic Plan",
        bool isFree = false,
        int maxOwnedGyms = 1,
        int maxCoachesPerGym = 5,
        int maxMembersPerGym = 50)
    {
        return new SubscriptionPlan
        {
            Id = id,
            Name = name,
            IsFree = isFree,
            MaxOwnedGyms = maxOwnedGyms,
            MaxCoaches = maxCoachesPerGym,
            MaxMembers = maxMembersPerGym,
            CreatedOn = DateTime.UtcNow,
            Prices = new List<PlanPrice>()
        };
    }

    public static PlanPrice CreatePlanPrice(
        int id = 1,
        int planId = 1,
        string countryCode = "US",
        string currencyCode = "USD",
        int durationMonths = 1,
        decimal amount = 50m)
    {
        return new PlanPrice
        {
            Id = id,
            PlanId = planId,
            CountryCode = countryCode,
            CurrencyCode = currencyCode,
            DurationMonths = durationMonths,
            Amount = amount,
            CreatedOn = DateTime.UtcNow,
            Plan = CreateSubscriptionPlan(id: planId)
        };
    }
}
