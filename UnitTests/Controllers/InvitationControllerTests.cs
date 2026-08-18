using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class InvitationControllerTests
{
    private readonly Mock<IInvitationService> _service;
    private readonly Mock<ILogger<InvitationController>> _logger;
    private readonly InvitationController _sut;

    public InvitationControllerTests()
    {
        _service = new Mock<IInvitationService>();
        _logger = new Mock<ILogger<InvitationController>>();
        _sut = new InvitationController(_service.Object, _logger.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<InvitationRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<InvitationRDTO>
            {
                new() { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Pending }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<InvitationRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetMyInvitationsAsync

    [Fact]
    public async Task GetMyInvitationsAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new GetMyInvitationsPagedReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<InvitationRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<InvitationRDTO>
            {
                new() { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Pending }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetMyInvitationsAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<InvitationRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenInvitationExists()
    {
        var invitation = new InvitationRDTO { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Pending };
        _service.Setup(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<InvitationRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
        _service.Verify(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenDataIsValid()
    {
        var cdto = new InvitationCDTO
        {
            GymId = 1,
            UserId = 5,
            GymRole = GymRole.Member,
            Membership = new InvitationMembershipDTO { MembershipPlanId = 1, DiscountAmount = 0 }
        };
        var rDto = new InvitationRDTO { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Pending };

        _service.Setup(s => s.CreateInvitationAsync(cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.CreateAsync(cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<InvitationRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("UserId", "Required");
        var cdto = new InvitationCDTO { GymId = 1, UserId = 0, GymRole = GymRole.Member };

        var result = await _sut.CreateAsync(cdto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region AcceptAsync

    [Fact]
    public async Task AcceptAsync_ShouldReturnOk_WhenInvitationAccepted()
    {
        var rDto = new InvitationRDTO { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Accepted };
        _service.Setup(s => s.AcceptInvitationAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.AcceptAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<InvitationRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(InvitationStatus.Accepted);
    }

    #endregion

    #region RejectAsync

    [Fact]
    public async Task RejectAsync_ShouldReturnOk_WhenInvitationRejected()
    {
        var rDto = new InvitationRDTO { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Rejected };
        _service.Setup(s => s.RejectInvitationAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.RejectAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<InvitationRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(InvitationStatus.Rejected);
    }

    #endregion

    #region CancelAsync

    [Fact]
    public async Task CancelAsync_ShouldReturnOk_WhenInvitationCancelled()
    {
        var rDto = new InvitationRDTO { Id = 1, GymId = 1, UserId = 5, GymRole = GymRole.Member, Status = InvitationStatus.Cancelled };
        _service.Setup(s => s.CancelInvitationAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.CancelAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<InvitationRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(InvitationStatus.Cancelled);
    }

    #endregion
}
