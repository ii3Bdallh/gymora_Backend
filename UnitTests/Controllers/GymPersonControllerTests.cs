using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Domain.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class GymPersonControllerTests
{
    private readonly Mock<IGymPersonService> _service;
    private readonly Mock<ILogger<GymPersonController>> _logger;
    private readonly GymPersonController _sut;

    public GymPersonControllerTests()
    {
        _service = new Mock<IGymPersonService>();
        _logger = new Mock<ILogger<GymPersonController>>();
        _sut = new GymPersonController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<GymPersonRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<GymPersonRDTO>
            {
                new()
                {
                    Id = 1,
                    GymId = 1,
                    PersonType = PersonType.Member,
                    Name = "John Doe",
                    PhoneNumber = "+1234567890",
                    AccessStatus = GymPersonAccessStatus.Active
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<GymPersonRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
        response.Data.Items.First().Name.Should().Be("John Doe");
    }

    #endregion

    #region GetMeAsync

    [Fact]
    public async Task GetMeAsync_ShouldReturnOk_WhenProfileExists()
    {
        var entity = new GymPersonRDTO
        {
            Id = 5,
            GymId = 1,
            PersonType = PersonType.Member,
            Name = "Current User Person",
            PhoneNumber = "+1234567890",
            AccessStatus = GymPersonAccessStatus.Active
        };

        _service.Setup(s => s.GetMeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetMeAsync();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(5);
        _service.Verify(s => s.GetMeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenEntityExists()
    {
        var entity = new GymPersonRDTO
        {
            Id = 1,
            GymId = 1,
            PersonType = PersonType.Member,
            Name = "John Doe",
            PhoneNumber = "+1234567890",
            AccessStatus = GymPersonAccessStatus.Active
        };

        _service.Setup(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
        _service.Verify(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenCreatedSuccessfully()
    {
        var dto = new GymPersonCDTO
        {
            PersonType = PersonType.Member,
            Name = "Jane Doe",
            PhoneNumber = "+1987654321"
        };
        var created = new GymPersonRDTO
        {
            Id = 2,
            GymId = 1,
            PersonType = PersonType.Member,
            Name = "Jane Doe",
            PhoneNumber = "+1987654321",
            AccessStatus = GymPersonAccessStatus.Active
        };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.CreateAsync(dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(2);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnOk_WhenUpdatedSuccessfully()
    {
        var dto = new GymPersonUDTO
        {
            PersonType = PersonType.Member,
            Name = "Jane Doe Updated",
            PhoneNumber = "+1987654321"
        };
        var updated = new GymPersonRDTO
        {
            Id = 2,
            GymId = 1,
            PersonType = PersonType.Member,
            Name = "Jane Doe Updated",
            PhoneNumber = "+1987654321",
            AccessStatus = GymPersonAccessStatus.Active
        };

        _service.Setup(s => s.UpdateAsync(2, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(2, dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Jane Doe Updated");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnOk_WhenDeletedSuccessfully()
    {
        var deleted = new GymPersonRDTO
        {
            Id = 1,
            GymId = 1,
            Name = "John Doe"
        };

        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleted);

        var result = await _sut.DeleteAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
    }

    #endregion

    #region LinkAccountToGymAsync

    [Fact]
    public async Task LinkAccountToGymAsync_ShouldReturnOk_WhenLinkedSuccessfully()
    {
        var inviteCode = Guid.NewGuid();
        var linked = new GymPersonRDTO { Id = 1, GymId = 10, UserId = 5 };

        _service.Setup(s => s.LinkAccountToGymAsync(10, inviteCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(linked);

        var result = await _sut.LinkAccountToGymAsync(10, inviteCode);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.UserId.Should().Be(5);
    }

    #endregion

    #region PaySalaryAsync

    [Fact]
    public async Task PaySalaryAsync_ShouldReturnOk_WhenPaidSuccessfully()
    {
        _service.Setup(s => s.PaySalaryAsync(1, null, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.PaySalaryAsync(1, null, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be("Salary payment initiated successfully.");
    }

    #endregion

    #region RenewMembershipAsync

    [Fact]
    public async Task RenewMembershipAsync_ShouldReturnOk_WhenRenewedSuccessfully()
    {
        var dto = new RenewMembershipDTO { MembershipPlanId = 1, PricePaid = 100m, FinalAmount = 100m };
        var renewed = new GymPersonRDTO { Id = 1, PersonType = PersonType.Member };

        _service.Setup(s => s.RenewMemberSubscriptionAsync(1, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renewed);

        var result = await _sut.RenewMembershipAsync(1, dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ChangeStatusAsync

    [Fact]
    public async Task ChangeStatusAsync_ShouldReturnOk_WhenStatusUpdated()
    {
        var dto = new UpdateAccessStatusDTO { Status = GymPersonAccessStatus.Suspended };
        var updated = new GymPersonRDTO { Id = 1, AccessStatus = GymPersonAccessStatus.Suspended };

        _service.Setup(s => s.UpdateAccessStatusAsync(1, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.ChangeStatusAsync(1, dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymPersonRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.AccessStatus.Should().Be(GymPersonAccessStatus.Suspended);
    }

    #endregion

    #region LeaveGymAsync

    [Fact]
    public async Task LeaveGymAsync_ShouldReturnOk_WhenLeftSuccessfully()
    {
        _service.Setup(s => s.LeaveGymAsync(10, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.LeaveGymAsync(10);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be("Successfully left the gym.");
    }

    #endregion
}
