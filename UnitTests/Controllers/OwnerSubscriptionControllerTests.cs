using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class OwnerSubscriptionControllerTests
{
    private readonly Mock<IOwnerSubscriptionService> _service;
    private readonly Mock<ICurrentPlanService> _currentPlanService;
    private readonly CurrentUser _currentUser;
    private readonly Mock<ILogger<OwnerSubscriptionController>> _logger;
    private readonly OwnerSubscriptionController _sut;

    public OwnerSubscriptionControllerTests()
    {
        _service = new Mock<IOwnerSubscriptionService>();
        _currentPlanService = new Mock<ICurrentPlanService>();
        _currentUser = new CurrentUser { UserId = 1, IsAuthenticated = true };
        _logger = new Mock<ILogger<OwnerSubscriptionController>>();
        _sut = new OwnerSubscriptionController(_logger.Object, _service.Object, _currentPlanService.Object, _currentUser);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<OwnerSubscriptionRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<OwnerSubscriptionRDTO>
            {
                new()
                {
                    Id = 1,
                    PlanId = 1,
                    PlanPriceId = 1,
                    AmountPaid = 50m,
                    CurrencyCode = "USD",
                    Status = OwnerSubscriptionStatus.Active
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<OwnerSubscriptionRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenEntityExists()
    {
        var entity = new OwnerSubscriptionRDTO
        {
            Id = 1,
            PlanId = 1,
            PlanPriceId = 1,
            AmountPaid = 50m,
            CurrencyCode = "USD",
            Status = OwnerSubscriptionStatus.Active
        };
        _service.Setup(s => s.GetByIdAsync(1, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<OwnerSubscriptionRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
    }

    #endregion

    #region GetMySubscriptionsAsync

    [Fact]
    public async Task GetMySubscriptionsAsync_ShouldReturnOk_WhenUserHasSubscription()
    {
        var planResult = new CurrentPlanResult
        {
            PlanId = 1,
            PlanName = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxMembersPerGym = 50,
            MaxCoachesPerGym = 5,
            SubscriptionStatus = OwnerSubscriptionStatus.Active
        };

        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planResult);

        var result = await _sut.GetMySubscriptionsAsync();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CurrentPlanResult>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.PlanId.Should().Be(1);
    }

    [Fact]
    public async Task GetMySubscriptionsAsync_ShouldReturnOk_WhenUserIsOnFreePlan()
    {
        var planResult = new CurrentPlanResult
        {
            PlanId = 0,
            PlanName = "Free",
            IsFree = true,
            MaxOwnedGyms = 1,
            MaxMembersPerGym = 10,
            MaxCoachesPerGym = 2
        };

        _currentPlanService.Setup(s => s.GetCurrentPlanAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planResult);

        var result = await _sut.GetMySubscriptionsAsync();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CurrentPlanResult>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.IsFree.Should().BeTrue();
    }

    #endregion
}
