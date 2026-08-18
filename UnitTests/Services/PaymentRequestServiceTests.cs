using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service;
using Domain.Enum;
using Domain.Events;
using Domain.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class PaymentRequestServiceTests
{
    private readonly Mock<IPaymentRequestRepo> _repo;
    private readonly Mock<ISubscriptionPlanRepo> _subscriptionPlanRepo;
    private readonly Mock<ICouponService> _couponService;
    private readonly Mock<IOwnerSubscriptionRepo> _ownerSubscriptionRepo;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Mock<IStorageService> _storageService;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<PaymentRequestService>> _logger;
    private readonly PaymentRequestService _sut;

    public PaymentRequestServiceTests()
    {
        _repo = new Mock<IPaymentRequestRepo>();
        _subscriptionPlanRepo = new Mock<ISubscriptionPlanRepo>();
        _couponService = new Mock<ICouponService>();
        _ownerSubscriptionRepo = new Mock<IOwnerSubscriptionRepo>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _storageService = Mocks.StorageService();
        _currentUser = Mocks.DefaultCurrentUser();
        _logger = Mocks.PaymentRequestLogger();

        _sut = new PaymentRequestService(
            _repo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object,
            _storageService.Object,
            _subscriptionPlanRepo.Object,
            _couponService.Object,
            _ownerSubscriptionRepo.Object,
            _currentPlanService.Object);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldReturnCreatedEntity_WhenDataIsValid()
    {
        var planPrice = TestData.CreatePlanPrice(amount: 100m);
        var entity = TestData.CreatePaymentRequest(originalAmount: 100m, finalAmount: 100m);
        var rDto = new PaymentRequestRDTO { Id = 1, OriginalAmount = 100m, FinalAmount = 100m };

        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _mapper.Setup(m => m.Map<PaymentRequest>(It.IsAny<PaymentRequestCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<PaymentRequestRDTO>(It.IsAny<PaymentRequest>()))
            .Returns(rDto);
        _repo.Setup(r => r.AddAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<PaymentCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cdto = new PaymentRequestCDTO
        {
            PlanPriceId = 1,
            PlanId = 1,
            CouponCode = null
        };

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenUserHasPendingRequest()
    {
        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var cdto = new PaymentRequestCDTO { PlanPriceId = 1, PlanId = 1 };

        var act = async () => await _sut.AddAsync(cdto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*pending payment request*");
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenPlanPriceNotFound()
    {
        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanPrice?)null);

        var cdto = new PaymentRequestCDTO { PlanPriceId = 999, PlanId = 1 };

        var act = async () => await _sut.AddAsync(cdto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid subscription plan*");
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenUserHasActiveSubscription()
    {
        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.CreatePlanPrice());
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = false, PlanId = 1 });

        var cdto = new PaymentRequestCDTO { PlanPriceId = 1, PlanId = 1 };

        var act = async () => await _sut.AddAsync(cdto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*active subscription*");
    }

    [Fact]
    public async Task AddAsync_ShouldApplyCoupon_WhenCouponCodeProvided()
    {
        var planPrice = TestData.CreatePlanPrice(amount: 100m);
        var entity = TestData.CreatePaymentRequest(
            couponId: 1,
            originalAmount: 100m,
            discountAmount: 20m,
            finalAmount: 80m);
        var rDto = new PaymentRequestRDTO { Id = 1, OriginalAmount = 100m, DiscountAmount = 20m, FinalAmount = 80m };

        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _couponService.Setup(s => s.ValidateCouponAsync("COUPON20", 100m, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CouponValidationResult.Success(1, 20m));
        _repo.Setup(r => r.HasUsedThisCouponBeforeAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _couponService.Setup(s => s.IncrementUsageAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<PaymentRequest>(It.IsAny<PaymentRequestCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<PaymentRequestRDTO>(It.IsAny<PaymentRequest>()))
            .Returns(rDto);
        _repo.Setup(r => r.AddAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<PaymentCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cdto = new PaymentRequestCDTO
        {
            PlanPriceId = 1,
            PlanId = 1,
            CouponCode = "COUPON20"
        };

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        _couponService.Verify(s => s.IncrementUsageAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenCouponAlreadyUsed()
    {
        var planPrice = TestData.CreatePlanPrice(amount: 100m);

        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _couponService.Setup(s => s.ValidateCouponAsync("COUPON20", 100m, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CouponValidationResult.Success(1, 20m));
        _repo.Setup(r => r.HasUsedThisCouponBeforeAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var cdto = new PaymentRequestCDTO
        {
            PlanPriceId = 1,
            PlanId = 1,
            CouponCode = "COUPON20"
        };

        var act = async () => await _sut.AddAsync(cdto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already used this coupon*");
    }

    [Fact]
    public async Task AddAsync_ShouldRollback_WhenExceptionOccurs()
    {
        var planPrice = TestData.CreatePlanPrice(amount: 100m);

        _repo.Setup(r => r.HasPendingRequestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _mapper.Setup(m => m.Map<PaymentRequest>(It.IsAny<PaymentRequestCDTO>()))
            .Throws(new InvalidOperationException("Mapping failed"));

        var cdto = new PaymentRequestCDTO { PlanPriceId = 1, PlanId = 1 };

        var act = async () => await _sut.AddAsync(cdto);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ApproveAsync

    [Fact]
    public async Task ApproveAsync_ShouldReturnApprovedEntity_WhenValid()
    {
        var entity = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Pending);
        var rDto = new PaymentRequestRDTO
        {
            Id = 1,
            Status = PaymentRequestStatus.Approved
        };

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<PaymentRequestRDTO>(It.IsAny<PaymentRequest>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<PaymentApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ApproveAsync(1, new PaymentRequestApprove { ReviewNotes = "Looks good" });

        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentRequestStatus.Approved);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenPaymentNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((PaymentRequest?)null);

        var act = async () => await _sut.ApproveAsync(999, new PaymentRequestApprove());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenPaymentIsNotPending()
    {
        var entity = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Approved);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);

        var act = async () => await _sut.ApproveAsync(1, new PaymentRequestApprove());

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Only pending*");
    }

    #endregion

    #region RejectAsync

    [Fact]
    public async Task RejectAsync_ShouldReturnRejectedEntity_WhenValid()
    {
        var entity = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Pending);
        var rDto = new PaymentRequestRDTO
        {
            Id = 1,
            Status = PaymentRequestStatus.Rejected
        };

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<PaymentRequestRDTO>(It.IsAny<PaymentRequest>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<PaymentRejectedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RejectAsync(1, new PaymentRequestReject { RejectionReason = "Invalid proof" });

        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentRequestStatus.Rejected);
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenPaymentNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((PaymentRequest?)null);

        var act = async () => await _sut.RejectAsync(999, new PaymentRequestReject { RejectionReason = "test" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenPaymentIsNotPending()
    {
        var entity = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Approved);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);

        var act = async () => await _sut.RejectAsync(1, new PaymentRequestReject { RejectionReason = "test" });

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Only pending*");
    }

    [Fact]
    public async Task RejectAsync_ShouldDecrementCouponUsage_WhenCouponExists()
    {
        var entity = TestData.CreatePaymentRequest(
            status: PaymentRequestStatus.Pending,
            couponId: 1);
        var rDto = new PaymentRequestRDTO { Id = 1, Status = PaymentRequestStatus.Rejected };

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<PaymentRequestRDTO>(It.IsAny<PaymentRequest>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _couponService.Setup(s => s.DecrementUsageAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<PaymentRejectedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RejectAsync(1, new PaymentRequestReject { RejectionReason = "test" });

        result.Should().NotBeNull();
        _couponService.Verify(s => s.DecrementUsageAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldRollback_WhenExceptionOccurs()
    {
        var entity = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Pending, couponId: 1);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(entity);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.RejectAsync(1, new PaymentRequestReject { RejectionReason = "test" });

        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
