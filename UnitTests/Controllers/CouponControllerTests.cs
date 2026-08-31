using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Controllers;

public class CouponControllerTests
{
    private readonly Mock<ICouponService> _service;
    private readonly Mock<ILogger<CouponController>> _logger;
    private readonly CouponController _sut;

    public CouponControllerTests()
    {
        _service = new Mock<ICouponService>();
        _logger = new Mock<ILogger<CouponController>>();
        _sut = new CouponController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<CouponRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<CouponRDTO>
            {
                new() { Id = 1, Code = "TEST10", Name = "Test Coupon" }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<CouponRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenCouponExists()
    {
        var coupon = new CouponRDTO { Id = 1, Code = "TEST10", Name = "Test" };
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Code.Should().Be("TEST10");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var cdto = new CouponCDTO
        {
            Code = "NEW10",
            Name = "New Coupon",
            DiscountType = Domain.Enum.DiscountType.FixedAmount,
            DiscountValue = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };
        var rDto = new CouponRDTO { Id = 1, Code = "NEW10", Name = "New Coupon" };

        _service.Setup(s => s.AddAsync(cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.CreateAsync(cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Code.Should().Be("NEW10");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Code", "Required");

        var cdto = new CouponCDTO { Code = null!, Name = "Test" };

        var result = await _sut.CreateAsync(cdto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var udto = new CouponUDTO
        {
            Name = "Updated",
            ValidTo = DateTime.UtcNow.AddDays(60),
        };
        var rDto = new CouponRDTO { Id = 1, Name = "Updated" };

        _service.Setup(s => s.UpdateAsync(1, udto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.UpdateAsync(1, udto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.UpdateAsync(1, new CouponUDTO { Name = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnOk_WhenCouponExists()
    {
        var rDto = new CouponRDTO { Id = 1, Code = "TEST10" };
        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.DeleteAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ValidateCouponAsync

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnOk_WhenCouponIsValid()
    {
        var validationResult = CouponValidationResult.Success(1, 10m);
        _service.Setup(s => s.ValidateCouponAsync("TEST10", 100m, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var result = await _sut.ValidateCouponAsync("TEST10", 100m);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponValidationResult>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.IsValid.Should().BeTrue();
    }

    #endregion
}
