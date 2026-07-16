using Api.Controllers;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class SubscriptionPlanControllerTests
{
    private readonly Mock<ISubscriptionPlanService> _service;
    private readonly Mock<ILogger<SubscriptionPlanController>> _logger;
    private readonly SubscriptionPlanController _sut;

    public SubscriptionPlanControllerTests()
    {
        _service = new Mock<ISubscriptionPlanService>();
        _logger = new Mock<ILogger<SubscriptionPlanController>>();
        _sut = new SubscriptionPlanController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<SubscriptionPlanRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<SubscriptionPlanRDTO>
            {
                new()
                {
                    Id = 1,
                    Name = "Basic",
                    IsFree = false,
                    MaxOwnedGyms = 1,
                    MaxCoachesPerGym = 5,
                    MaxMembersPerGym = 50
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<SubscriptionPlanRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenPlanExists()
    {
        var plan = new SubscriptionPlanRDTO
        {
            Id = 1,
            Name = "Basic",
            IsFree = false,
            MaxOwnedGyms = 1,
            MaxCoachesPerGym = 5,
            MaxMembersPerGym = 50
        };
        _service.Setup(s => s.GetByIdAsync(1, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SubscriptionPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Basic");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var cdto = new SubscriptionPlanCDTO
        {
            Name = "Premium",
            IsFree = false,
            MaxOwnedGyms = 5,
            MaxCoachesPerGym = 20,
            MaxMembersPerGym = 200
        };
        var rDto = new SubscriptionPlanRDTO { Id = 1, Name = "Premium" };

        _service.Setup(s => s.AddAsync(cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.CreateAsync(cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SubscriptionPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.CreateAsync(new SubscriptionPlanCDTO { Name = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var udto = new SubscriptionPlanUDTO
        {
            Name = "Updated",
            IsFree = false,
            MaxOwnedGyms = 2,
            MaxCoachesPerGym = 10,
            MaxMembersPerGym = 100,
            IsActive = true
        };
        var rDto = new SubscriptionPlanRDTO { Id = 1, Name = "Updated" };

        _service.Setup(s => s.UpdateAsync(1, udto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.UpdateAsync(1, udto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SubscriptionPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.UpdateAsync(1, new SubscriptionPlanUDTO { Name = null! });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnOk_WhenPlanExists()
    {
        var rDto = new SubscriptionPlanRDTO { Id = 1, Name = "Basic" };
        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.DeleteAsync(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SubscriptionPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region CreatePlanPriceAsync

    [Fact]
    public async Task CreatePlanPriceAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var cdto = new PlanPriceCDTO
        {
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 50m
        };
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 50m };

        _service.Setup(s => s.AddPlanPriceAsync(1, cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.CreatePlanPriceAsync(1, cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PlanPriceRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePlanPriceAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("CountryCode", "Required");

        var result = await _sut.CreatePlanPriceAsync(1, new PlanPriceCDTO { CountryCode = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdatePlanPriceAsync

    [Fact]
    public async Task UpdatePlanPriceAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var udto = new PlanPriceUDTO
        {
            CountryCode = "US",
            CurrencyCode = "USD",
            DurationMonths = 1,
            Amount = 75m
        };
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 75m };

        _service.Setup(s => s.UpdatePlanPriceAsync(1, udto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.UpdatePlanPriceAsync(1, 1, udto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PlanPriceRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePlanPriceAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("CountryCode", "Required");

        var result = await _sut.UpdatePlanPriceAsync(1, 1, new PlanPriceUDTO { CountryCode = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeletePlanPriceAsync

    [Fact]
    public async Task DeletePlanPriceAsync_ShouldReturnOk_WhenPriceExists()
    {
        var rDto = new PlanPriceRDTO { Id = 1, PlanId = 1, Amount = 50m };
        _service.Setup(s => s.DeletePlanPriceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.DeletePlanPriceAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PlanPriceRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenPlanNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999, true, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("SubscriptionPlan with ID 999 was not found."));

        var act = async () => await _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
