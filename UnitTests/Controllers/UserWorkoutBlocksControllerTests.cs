using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class UserWorkoutBlocksControllerTests
{
    private readonly Mock<IUserWorkoutBlockService> _service;
    private readonly Mock<ILogger<UserWorkoutBlocksController>> _logger;
    private readonly UserWorkoutBlocksController _sut;

    public UserWorkoutBlocksControllerTests()
    {
        _service = new Mock<IUserWorkoutBlockService>();
        _logger = new Mock<ILogger<UserWorkoutBlocksController>>();
        _sut = new UserWorkoutBlocksController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new UserWorkoutBlockRDTO
        {
            Id = id,
            BlockedUserId = 10,
            BlockedUserName = "Blocked User",
            BlockedUntil = DateTime.UtcNow.AddDays(7),
            Reason = "Spamming workout plans"
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<UserWorkoutBlockRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.BlockedUserName.Should().Be("Blocked User");
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<UserWorkoutBlockRDTO>
        {
            Items = new List<UserWorkoutBlockRDTO>
            {
                new() { Id = 1, BlockedUserId = 10, Reason = "Violation" }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _service.Setup(s => s.GetPageAsync(req, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResult);

        // Act
        var result = await _sut.GetPaged(req, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<UserWorkoutBlockRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new UserWorkoutBlockCDTO { BlockedUserId = 10, DurationDays = 30, Reason = "Abuse" };
        var rDto = new UserWorkoutBlockRDTO { Id = 1, BlockedUserId = 10, Reason = "Abuse" };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<UserWorkoutBlockRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.BlockedUserId.Should().Be(10);
    }

    [Fact]
    public async Task BlockUser_ShouldReturnCreated_WhenBlockSucceeds()
    {
        // Arrange
        var dto = new UserWorkoutBlockCDTO { BlockedUserId = 15, DurationDays = 7, Reason = "Inappropriate content" };
        var rDto = new UserWorkoutBlockRDTO { Id = 2, BlockedUserId = 15, Reason = "Inappropriate content" };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.BlockUser(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<UserWorkoutBlockRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.BlockedUserId.Should().Be(15);
    }

    [Fact]
    public async Task UnblockUser_ShouldReturnOk_WhenUnblockSucceeds()
    {
        // Arrange
        int userId = 10;
        _service.Setup(s => s.UnblockUserAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UnblockUser(userId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Contain("unblocked");
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new UserWorkoutBlockUDTO { BlockedUntil = DateTime.UtcNow.AddDays(60), Reason = "Extended block" };
        var rDto = new UserWorkoutBlockRDTO { Id = id, Reason = "Extended block" };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<UserWorkoutBlockRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Reason.Should().Be("Extended block");
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new UserWorkoutBlockRDTO { Id = id };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<UserWorkoutBlockRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }
}
