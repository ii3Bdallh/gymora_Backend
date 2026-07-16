using Application.Cache;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Model;
using Application.Service;
using Application.Service.Entity;
using Domain.Events;
using Domain.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class SubscriptionPlanServiceTests
{
    private readonly Mock<ISubscriptionPlanRepo> _repo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<SubscriptionPlanService>> _logger;
    private readonly SubscriptionPlanService _sut;

    public SubscriptionPlanServiceTests()
    {
        _repo = new Mock<ISubscriptionPlanRepo>();
        _unitOfWork = Mocks.UnitOfWork();
        _mapper = Mocks.Mapper();
        _cacheService = Mocks.CacheService();
        _publishEndpoint = Mocks.PublishEndpoint();
        _currentUser = Mocks.SuperAdminCurrentUser();
        _logger = Mocks.SubscriptionPlanLogger();

        _sut = new SubscriptionPlanService(
            _repo.Object,
            _unitOfWork.Object,
            _mapper.Object,
            _cacheService.Object,
            _publishEndpoint.Object,
            _currentUser,
            _logger.Object);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldReturnCreatedEntity_WhenDataIsValid()
    {
        var entity = TestData.CreateSubscriptionPlan();
        var cdto = new SubscriptionPlanCDTO { Name = "Premium", IsFree = false };
        var rDto = new SubscriptionPlanRDTO { Id = 1, Name = "Premium" };

        _repo.Setup(r => r.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<SubscriptionPlan>(It.IsAny<SubscriptionPlanCDTO>()))
            .Returns(entity);
        _mapper.Setup(m => m.Map<SubscriptionPlanRDTO>(It.IsAny<SubscriptionPlan>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(cdto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Premium");
    }

    #endregion

    #region AddPlanPriceAsync

    [Fact]
    public async Task AddPlanPriceAsync_ShouldReturnCreatedPrice_WhenPlanExists()
    {
        var plan = TestData.CreateSubscriptionPlan();
        var planPrice = TestData.CreatePlanPrice();
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 50m };

        _repo.Setup(r => r.GetByIdAsync(1, true, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(plan);
        _mapper.Setup(m => m.Map<PlanPrice>(It.IsAny<PlanPriceCDTO>()))
            .Returns(planPrice);
        _repo.Setup(r => r.AddPlanPriceAsync(It.IsAny<PlanPrice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _mapper.Setup(m => m.Map<PlanPriceRDTO>(It.IsAny<PlanPrice>()))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cdto = new PlanPriceCDTO
        {
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m
        };

        var result = await _sut.AddPlanPriceAsync(1, cdto);

        result.Should().NotBeNull();
        result.PlanId.Should().Be(1);
        _repo.Verify(r => r.AddPlanPriceAsync(It.IsAny<PlanPrice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPlanPriceAsync_ShouldThrow_WhenPlanNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(999, true, false, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync((SubscriptionPlan?)null);

        var cdto = new PlanPriceCDTO { CountryCode = "US", CurrencyCode = "USD", DurationMonths = 1, Amount = 50m };

        var act = async () => await _sut.AddPlanPriceAsync(999, cdto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*SubscriptionPlan*");
    }

    #endregion

    #region GetPlanPriceByIdAsync

    [Fact]
    public async Task GetPlanPriceByIdAsync_ShouldReturnPrice_WhenExists()
    {
        var planPrice = TestData.CreatePlanPrice();
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 50m };

        _repo.Setup(r => r.GetPlanPriceByIdAsync(1, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _mapper.Setup(m => m.Map<PlanPriceRDTO>(planPrice))
            .Returns(rDto);

        var result = await _sut.GetPlanPriceByIdAsync(1);

        result.Should().NotBeNull();
        result!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetPlanPriceByIdAsync_ShouldThrow_WhenNotFound()
    {
        _repo.Setup(r => r.GetPlanPriceByIdAsync(999, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanPrice?)null);

        var act = async () => await _sut.GetPlanPriceByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*PlanPrice*");
    }

    #endregion

    #region UpdatePlanPriceAsync

    [Fact]
    public async Task UpdatePlanPriceAsync_ShouldReturnUpdatedPrice_WhenValid()
    {
        var planPrice = TestData.CreatePlanPrice();
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 75m };

        _repo.Setup(r => r.GetPlanPriceByIdAsync(1, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _mapper.Setup(m => m.Map<PlanPriceUDTO, PlanPrice>(It.IsAny<PlanPriceUDTO>(), planPrice))
            .Returns(planPrice);
        _mapper.Setup(m => m.Map<PlanPriceRDTO>(planPrice))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var udto = new PlanPriceUDTO
        {
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 75m
        };

        var result = await _sut.UpdatePlanPriceAsync(1, udto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdatePlanPriceAsync_ShouldThrow_WhenNotFound()
    {
        _repo.Setup(r => r.GetPlanPriceByIdAsync(999, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanPrice?)null);

        var udto = new PlanPriceUDTO
        {
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 75m
        };

        var act = async () => await _sut.UpdatePlanPriceAsync(999, udto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*PlanPrice*");
    }

    #endregion

    #region DeletePlanPriceAsync

    [Fact]
    public async Task DeletePlanPriceAsync_ShouldReturnDeletedPrice_WhenExists()
    {
        var planPrice = TestData.CreatePlanPrice();
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 50m };

        _repo.Setup(r => r.GetPlanPriceByIdAsync(1, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _repo.Setup(r => r.DeletePlanPriceAsync(planPrice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planPrice);
        _mapper.Setup(m => m.Map<PlanPriceRDTO>(planPrice))
            .Returns(rDto);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _publishEndpoint.Setup(p => p.Publish(It.IsAny<EntityChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeletePlanPriceAsync(1);

        result.Should().NotBeNull();
        _repo.Verify(r => r.DeletePlanPriceAsync(planPrice, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePlanPriceAsync_ShouldThrow_WhenNotFound()
    {
        _repo.Setup(r => r.GetPlanPriceByIdAsync(999, true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanPrice?)null);

        var act = async () => await _sut.DeletePlanPriceAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*PlanPrice*");
    }

    #endregion
}
