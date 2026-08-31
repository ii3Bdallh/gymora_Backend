using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using FluentAssertions;
using Gymora.Contracts.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class GymControllerTests
{
    private readonly Mock<IGymService> _service;
    private readonly Mock<ILogger<GymController>> _logger;
    private readonly GymController _sut;

    public GymControllerTests()
    {
        _service = new Mock<IGymService>();
        _logger = new Mock<ILogger<GymController>>();
        _sut = new GymController(_logger.Object, _service.Object);
    }

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<GymRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<GymRDTO>
            {
                new()
                {
                    Id = 1,
                    Name = "Test Gym",
                    Status = GymStatus.Active,
                    OwnerUserId = 2
                }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPagedAsync(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<GymRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOk_WhenGymExists()
    {
        var gymDto = new GymRDTO
        {
            Id = 1,
            Name = "Test Gym",
            Status = GymStatus.Active,
            OwnerUserId = 2
        };

        _service.Setup(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gymDto);

        var result = await _sut.GetByIdAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
        response.Data.Name.Should().Be("Test Gym");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenCreationIsSuccessful()
    {
        var cdto = new GymCDTO { Name = "New Gym", Latitude = 30.0m, Longitude = 31.0m };
        var rdto = new GymRDTO { Id = 10, Name = "New Gym", Status = GymStatus.Active, OwnerUserId = 2 };

        _service.Setup(s => s.AddAsync(cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rdto);

        var result = await _sut.CreateAsync(cdto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(10);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        var udto = new GymUDTO { Name = "Updated Gym" };
        var rdto = new GymRDTO { Id = 1, Name = "Updated Gym", Status = GymStatus.Active };

        _service.Setup(s => s.UpdateAsync(1, udto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rdto);

        var result = await _sut.UpdateAsync(1, udto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Updated Gym");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnOk_WhenDeletionIsSuccessful()
    {
        var rdto = new GymRDTO { Id = 1, Name = "Deleted Gym" };

        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rdto);

        var result = await _sut.DeleteAsync(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ChangeOwnerAsync

    [Fact]
    public async Task ChangeOwnerAsync_ShouldReturnOk_WhenOwnerChangeSucceeds()
    {
        var dto = new ChangeOwnerDTO { GymId = 1, NewOwnerUserId = 5 };

        _service.Setup(s => s.ChangeOwnerOfGymAsync(1, 5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ChangeOwnerAsync(dto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be("Owner changed successfully.");
    }

    #endregion

    #region GetUserGymsAsync

    [Fact]
    public async Task GetUserGymsAsync_ShouldReturnOk_WhenDataExists()
    {
        var req = new UserGymsPagedReq { PageNumber = 1, PageSize = 10 };
        var gymsList = new UserGymsListRDTO
        {
            Gyms = new List<UserGymRDTO>
            {
                new() { GymId = 1, GymName = "User Gym 1", IsAccessible = true }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _service.Setup(s => s.GetUserGymsAsync(req, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gymsList);

        var result = await _sut.GetUserGymsAsync(req);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<UserGymsListRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Gyms.Should().HaveCount(1);
    }

    #endregion
}
