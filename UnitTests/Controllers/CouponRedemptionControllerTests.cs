using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class CouponRedemptionControllerTests
{
    private readonly Mock<ICouponRedemptionService> _service;
    private readonly Mock<ILogger<CouponRedemptionController>> _logger;
    private readonly CouponRedemptionController _sut;

    public CouponRedemptionControllerTests()
    {
        _service = new Mock<ICouponRedemptionService>();
        _logger = new Mock<ILogger<CouponRedemptionController>>();
        _sut = new CouponRedemptionController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<CouponRedemptionRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<CouponRedemptionRDTO>
            {
                new() { Id = 1, CouponId = 1, PaymentRequestId = 1, DiscountAmount = 10m }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<CouponRedemptionRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenEntityExists()
    {
        var entity = new CouponRedemptionRDTO { Id = 1, CouponId = 1, PaymentRequestId = 1, DiscountAmount = 10m };
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<CouponRedemptionRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
    }

    #endregion
}
