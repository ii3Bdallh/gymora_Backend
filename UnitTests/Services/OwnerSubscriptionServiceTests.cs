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

public class OwnerSubscriptionServiceTests
{
    private readonly Mock<IOwnerSubscriptionRepo> _repo;
    private readonly Mock<IPaymentRequestRepo> _paymentRequestRepo;
    private readonly Mock<ISubscriptionPlanRepo> _subscriptionPlanRepo;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<OwnerSubscriptionService>> _logger;
    private readonly OwnerSubscriptionService _sut;

    public OwnerSubscriptionServiceTests()
    {
        _repo = Mocks.OwnerSubscriptionRepo();
        _paymentRequestRepo = new Mock<IPaymentRequestRepo>();
        _subscriptionPlanRepo = new Mock<ISubscriptionPlanRepo>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _currentUser = Mocks.DefaultCurrentUser();
        _logger = Mocks.OwnerSubscriptionLogger();

        _sut = new OwnerSubscriptionService(
            _repo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object,
            _paymentRequestRepo.Object,
            _subscriptionPlanRepo.Object,
            _currentPlanService.Object);
    }

    #region CreateFromApprovedPaymentAsync

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldReturnCreatedSubscription_WhenPaymentIsValid()
    {
        var paymentRequest = TestData.CreatePaymentRequest(
            status: PaymentRequestStatus.Approved,
            planPriceId: 1);
        var planPrice = TestData.CreatePlanPrice(id: 1, durationMonths: 1, amount: 50m);
        var subscription = TestData.CreateOwnerSubscription();
        var rDto = new OwnerSubscriptionRDTO
        {
            Id = 1,
            PlanId = 1,
            PlanPriceId = 1,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            GraceEndDate = DateTime.UtcNow.AddMonths(1).AddDays(7),
            Status = OwnerSubscriptionStatus.Active
        };

        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(paymentRequest);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(paymentRequest.CreatedById, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true, PlanId = 0 });
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _repo.Setup(r => r.AddAsync(It.IsAny<OwnerSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _mapper.Setup(m => m.Map<OwnerSubscriptionRDTO>(It.IsAny<OwnerSubscription>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<SubscriptionActivatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateFromApprovedPaymentAsync(1);

        result.Should().NotBeNull();
        result.PlanId.Should().Be(1);
        _repo.Verify(r => r.AddAsync(It.IsAny<OwnerSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpoint.Verify(p => p.Publish(It.IsAny<SubscriptionActivatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldThrow_WhenPaymentNotFound()
    {
        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(999, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((PaymentRequest?)null);

        var act = async () => await _sut.CreateFromApprovedPaymentAsync(999);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*not approved*");
    }

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldThrow_WhenPaymentIsNotApproved()
    {
        var paymentRequest = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Pending);

        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(paymentRequest);

        var act = async () => await _sut.CreateFromApprovedPaymentAsync(1);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*not approved*");
    }

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldThrow_WhenUserHasActiveSubscription()
    {
        var paymentRequest = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Approved);

        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(paymentRequest);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = false, PlanId = 1 });

        var act = async () => await _sut.CreateFromApprovedPaymentAsync(1);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*active subscription*");
    }

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldThrow_WhenPlanPriceNotFound()
    {
        var paymentRequest = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Approved, planPriceId: 999);

        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(paymentRequest);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(999, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanPrice?)null);

        var act = async () => await _sut.CreateFromApprovedPaymentAsync(1);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*Invalid subscription plan price*");
    }

    [Fact]
    public async Task CreateFromApprovedPaymentAsync_ShouldSetCorrectDates_WhenPlanIsValid()
    {
        var paymentRequest = TestData.CreatePaymentRequest(status: PaymentRequestStatus.Approved);
        var planPrice = TestData.CreatePlanPrice(durationMonths: 3);
        var subscription = TestData.CreateOwnerSubscription();
        var rDto = new OwnerSubscriptionRDTO
        {
            Id = 1,
            PlanId = 1,
            PlanPriceId = 1,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            Status = OwnerSubscriptionStatus.Active
        };

        _paymentRequestRepo.Setup(r => r.GetByIdIgnoringSecurityAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(paymentRequest);
        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrentPlanResult { IsFree = true });
        _subscriptionPlanRepo.Setup(r => r.GetPlanPriceByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _repo.Setup(r => r.AddAsync(It.IsAny<OwnerSubscription>(), It.IsAny<CancellationToken>()))
            .Callback<OwnerSubscription, CancellationToken>((e, _) =>
            {
                e.StartDate = DateTime.UtcNow;
                e.EndDate = e.StartDate.AddMonths(3);
                e.GraceEndDate = e.EndDate.AddDays(7);
            })
            .ReturnsAsync(subscription);
        _mapper.Setup(m => m.Map<OwnerSubscriptionRDTO>(It.IsAny<OwnerSubscription>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<SubscriptionActivatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateFromApprovedPaymentAsync(1);

        result.Should().NotBeNull();
        _repo.Verify(r => r.AddAsync(It.Is<OwnerSubscription>(e =>
            e.EndDate > e.StartDate &&
            e.GraceEndDate > e.EndDate), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AddAsync (inherited)

    [Fact]
    public async Task AddAsync_ShouldSetCreatedById_WhenAddingSubscription()
    {
        var entity = TestData.CreateOwnerSubscription();
        var cdto = new OwnerSubscriptionCDTO
        {
            PlanId = 1,
            PlanPriceId = 1,
            AmountPaid = 50m,
            CurrencyCode = "USD"
        };
        var rDto = new OwnerSubscriptionRDTO
        {
            Id = 1,
            PlanId = 1,
            CreatedById = 1
        };

        _repo.Setup(r => r.AddAsync(It.IsAny<OwnerSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<OwnerSubscription>(It.IsAny<OwnerSubscriptionCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<OwnerSubscriptionRDTO>(It.IsAny<OwnerSubscription>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        _repo.Verify(r => r.AddAsync(It.IsAny<OwnerSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
