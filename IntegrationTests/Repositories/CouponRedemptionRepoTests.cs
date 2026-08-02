using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Infrastructure.Repo;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class CouponRedemptionRepoTests : IDisposable
{
    private readonly Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly CouponRedemptionRepo _sut;
    private readonly string _dbName;

    public CouponRedemptionRepoTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _context = InMemoryDbContextFactory.Create(_dbName);
        var currentUser = InMemoryDbContextFactory.SuperAdminCurrentUser();
        _sut = new CouponRedemptionRepo(
            _context,
            InMemoryDbContextFactory.Logger<Infrastructure.Repo.CouponRedemptionRepo>().Object,
            new Infrastructure.Cache.QueryCache(),
            currentUser);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistEntity_WhenDataIsValid()
    {
        var coupon = new Coupon
        {
            Code = "TEST10",
            Name = "Test",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoaches = 5,
            MaxMembers = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var redemption = new CouponRedemption
        {
            CouponId = coupon.Id,
            PaymentRequestId = paymentRequest.Id,
            DiscountAmount = 10m,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _sut.AddAsync(redemption);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var fromDb = await _context.CouponRedemption.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.DiscountAmount.Should().Be(10m);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var coupon = new Coupon
        {
            Code = "TEST",
            Name = "Test",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var plan = new SubscriptionPlan
        {
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoaches = 5,
            MaxMembers = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var paymentRequest = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 50m,
            FinalAmount = 50m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(paymentRequest);
        await _context.SaveChangesAsync();

        var redemption = new CouponRedemption
        {
            CouponId = coupon.Id,
            PaymentRequestId = paymentRequest.Id,
            DiscountAmount = 5m,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };
        _context.CouponRedemption.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(redemption.Id);

        result.Should().NotBeNull();
        result!.DiscountAmount.Should().Be(5m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveRedemptions()
    {
        var coupon = new Coupon
        {
            Code = "T",
            Name = "T",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var plan = new SubscriptionPlan
        {
            Name = "P",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoaches = 5,
            MaxMembers = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var pr = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 50m,
            FinalAmount = 50m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(pr);
        await _context.SaveChangesAsync();

        _context.CouponRedemption.AddRange(
            new CouponRedemption
            {
                CouponId = coupon.Id,
                PaymentRequestId = pr.Id,
                DiscountAmount = 5m,
                CreatedById = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            },
            new CouponRedemption
            {
                CouponId = coupon.Id,
                PaymentRequestId = pr.Id,
                DiscountAmount = 10m,
                CreatedById = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            }
        );
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region DeleteAsync (Soft Delete)

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var coupon = new Coupon
        {
            Code = "D",
            Name = "D",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 5m,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RowVersion = [1, 2, 3]
        };
        _context.Coupon.Add(coupon);
        await _context.SaveChangesAsync();

        var plan = new SubscriptionPlan
        {
            Name = "P",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoaches = 5,
            MaxMembers = 50,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };
        _context.SubscriptionPlan.Add(plan);
        await _context.SaveChangesAsync();

        var pr = new PaymentRequest
        {
            PlanId = plan.Id,
            PlanPriceId = 0,
            OriginalAmount = 50m,
            FinalAmount = 50m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Approved,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedById = 1,
            StoredFilePath = string.Empty,
            RowVersion = [1, 2, 3]
        };
        _context.PaymentRequest.Add(pr);
        await _context.SaveChangesAsync();

        var redemption = new CouponRedemption
        {
            CouponId = coupon.Id,
            PaymentRequestId = pr.Id,
            DiscountAmount = 5m,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };
        _context.CouponRedemption.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _sut.DeleteAsync(redemption);
        await _context.SaveChangesAsync();

        result.IsActive.Should().BeFalse();
    }

    #endregion
}
