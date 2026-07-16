using Api.Controllers;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class PaymentRequestControllerTests
{
    private readonly Mock<IPaymentRequestService> _service;
    private readonly Mock<ILogger<PaymentRequestController>> _logger;
    private readonly PaymentRequestController _sut;

    public PaymentRequestControllerTests()
    {
        _service = new Mock<IPaymentRequestService>();
        _logger = new Mock<ILogger<PaymentRequestController>>();
        _sut = new PaymentRequestController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<PaymentRequestRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<PaymentRequestRDTO>
            {
                new()
                {
                    Id = 1,
                    PlanId = 1,
                    PlanPriceId = 1,
                    OriginalAmount = 100m,
                    FinalAmount = 100m,
                    CurrencyCode = "USD",
                    Status = PaymentRequestStatus.Pending
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<PaymentRequestRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenEntityExists()
    {
        var entity = new PaymentRequestRDTO
        {
            Id = 1,
            PlanId = 1,
            PlanPriceId = 1,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            CurrencyCode = "USD",
            Status = PaymentRequestStatus.Pending
        };
        _service.Setup(s => s.GetByIdAsync(1, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaymentRequestRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var rDto = new PaymentRequestRDTO
        {
            Id = 1,
            PlanId = 1,
            PlanPriceId = 1,
            OriginalAmount = 100m,
            FinalAmount = 100m,
            Status = PaymentRequestStatus.Pending
        };

        _service.Setup(s => s.AddAsync(It.IsAny<PaymentRequestCDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var cdto = new PaymentRequestCDTO
        {
            PlanId = 1,
            PlanPriceId = 1,
            File = null
        };

        var result = await _sut.CreateAsync(cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaymentRequestRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("PlanPriceId", "Required");

        var cdto = new PaymentRequestCDTO { PlanPriceId = 0, PlanId = 0 };

        var result = await _sut.CreateAsync(cdto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region ApproveAsync

    [Fact]
    public async Task ApproveAsync_ShouldReturnOk_WhenPaymentIsValid()
    {
        var rDto = new PaymentRequestRDTO
        {
            Id = 1,
            Status = PaymentRequestStatus.Approved
        };

        _service.Setup(s => s.ApproveAsync(1, It.IsAny<PaymentRequestApprove>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.ApproveAsync(1, new PaymentRequestApprove { ReviewNotes = "Approved" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaymentRequestRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(PaymentRequestStatus.Approved);
    }

    #endregion

    #region RejectAsync

    [Fact]
    public async Task RejectAsync_ShouldReturnOk_WhenPaymentIsValid()
    {
        var rDto = new PaymentRequestRDTO
        {
            Id = 1,
            Status = PaymentRequestStatus.Rejected
        };

        _service.Setup(s => s.RejectAsync(1, It.IsAny<PaymentRequestReject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.RejectAsync(1, new PaymentRequestReject { RejectionReason = "Invalid proof" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaymentRequestRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(PaymentRequestStatus.Rejected);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenPaymentNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999, true, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Payment request not found."));

        var act = async () => await _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrowApplicationException_WhenPaymentIsNotPending()
    {
        _service.Setup(s => s.ApproveAsync(1, It.IsAny<PaymentRequestApprove>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApplicationException("Only pending payment requests can be approved."));

        var act = async () => await _sut.ApproveAsync(1, new PaymentRequestApprove());

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("*Only pending*");
    }

    #endregion
}
