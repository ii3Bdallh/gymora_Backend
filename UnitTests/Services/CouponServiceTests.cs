using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Model;
using Application.Service;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class CouponServiceTests
{
    private readonly Mock<ICouponRepo> _couponRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<CouponService>> _logger;
    private readonly CouponService _sut;

    public CouponServiceTests()
    {
        _couponRepo = Mocks.CouponRepo();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _currentUser = Mocks.SuperAdminCurrentUser();
        _logger = Mocks.CouponLogger();

        _sut = new CouponService(
            _couponRepo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object);
    }

    #region ValidateCouponAsync

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnSuccess_WhenCouponIsValid()
    {
        var coupon = TestData.CreateCoupon(
            code: "VALID10",
            discountType: DiscountType.FixedAmount,
            discountValue: 10m,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validTo: DateTime.UtcNow.AddDays(30),
            usageLimit: 100,
            usedCount: 0);

        _couponRepo.Setup(r => r.GetByCodeAsync("VALID10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync("VALID10", 100m, 1);

        result.IsValid.Should().BeTrue();
        result.CouponId.Should().Be(coupon.Id);
        result.DiscountAmount.Should().Be(10m);
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnFailure_WhenCouponNotFound()
    {
        _couponRepo.Setup(r => r.GetByCodeAsync("NONEXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coupon?)null);

        var result = await _sut.ValidateCouponAsync("NONEXISTENT", 100m, 1);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnFailure_WhenCouponIsInactive()
    {
        var coupon = TestData.CreateCoupon(isActive: false);

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 100m, 1);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("inactive");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnFailure_WhenCouponIsExpired()
    {
        var coupon = TestData.CreateCoupon(
            validFrom: DateTime.UtcNow.AddDays(-30),
            validTo: DateTime.UtcNow.AddDays(-1));

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 100m, 1);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("valid period");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnFailure_WhenUsageLimitReached()
    {
        var coupon = TestData.CreateCoupon(usageLimit: 5, usedCount: 5);

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 100m, 1);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("usage limit");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnFailure_WhenOrderAmountBelowMinimum()
    {
        var coupon = TestData.CreateCoupon(minimumPurchaseAmount: 100m);

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 50m, 1);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("below the minimum");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldCalculatePercentageDiscount_WhenPercentageType()
    {
        var coupon = TestData.CreateCoupon(
            discountType: DiscountType.Percentage,
            discountValue: 20m,
            maxDiscountAmount: null);

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 200m, 1);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(40m);
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldCapDiscountAtMax_WhenMaxDiscountAmountExceeded()
    {
        var coupon = TestData.CreateCoupon(
            discountType: DiscountType.Percentage,
            discountValue: 50m,
            maxDiscountAmount: 25m);

        _couponRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.ValidateCouponAsync(coupon.Code, 100m, 1);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(25m);
    }

    #endregion

    #region IncrementUsageAsync

    [Fact]
    public async Task IncrementUsageAsync_ShouldIncrementUsedCount_WhenCouponExists()
    {
        var coupon = TestData.CreateCoupon(usedCount: 5, usageLimit: 10);

        _couponRepo.Setup(r => r.GetByIdAsync(1, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(coupon);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sut.IncrementUsageAsync(1);

        coupon.UsedCount.Should().Be(6);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementUsageAsync_ShouldNotIncrement_WhenCouponNotFound()
    {
        _couponRepo.Setup(r => r.GetByIdAsync(999, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((Coupon?)null);

        await _sut.IncrementUsageAsync(999);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IncrementUsageAsync_ShouldNotIncrement_WhenUsageLimitReached()
    {
        var coupon = TestData.CreateCoupon(usedCount: 10, usageLimit: 10);

        _couponRepo.Setup(r => r.GetByIdAsync(1, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(coupon);

        await _sut.IncrementUsageAsync(1);

        coupon.UsedCount.Should().Be(10);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DecrementUsageAsync

    [Fact]
    public async Task DecrementUsageAsync_ShouldDecrementUsedCount_WhenCouponExists()
    {
        var coupon = TestData.CreateCoupon(usedCount: 5);

        _couponRepo.Setup(r => r.GetByIdAsync(1, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(coupon);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sut.DecrementUsageAsync(1);

        coupon.UsedCount.Should().Be(4);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementUsageAsync_ShouldNotDecrement_WhenCouponNotFound()
    {
        _couponRepo.Setup(r => r.GetByIdAsync(999, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((Coupon?)null);

        await _sut.DecrementUsageAsync(999);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DecrementUsageAsync_ShouldNotDecrement_WhenUsedCountIsZero()
    {
        var coupon = TestData.CreateCoupon(usedCount: 0);

        _couponRepo.Setup(r => r.GetByIdAsync(1, true, true, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(coupon);

        await _sut.DecrementUsageAsync(1);

        coupon.UsedCount.Should().Be(0);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
