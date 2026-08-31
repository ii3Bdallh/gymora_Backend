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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class SessionsControllerTests
{
    private readonly Mock<ISessionService> _service;
    private readonly Mock<ILogger<SessionsController>> _logger;
    private readonly SessionsController _sut;

    public SessionsControllerTests()
    {
        _service = new Mock<ISessionService>();
        _logger = new Mock<ILogger<SessionsController>>();
        _sut = new SessionsController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new SessionRDTO
        {
            Id = id,
            SessionName = "Chest & Triceps",
            DayNumber = 1,
            WorkoutPlanId = 2,
            IsApproved = true
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SessionRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.SessionName.Should().Be("Chest & Triceps");
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<SessionRDTO>
        {
            Items = new List<SessionRDTO>
            {
                new() { Id = 1, SessionName = "Leg Day", DayNumber = 2 }
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
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<SessionRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new SessionCDTO { SessionName = "Back & Biceps", DayNumber = 1, WorkoutPlanId = 1 };
        var rDto = new SessionRDTO { Id = 1, SessionName = "Back & Biceps", DayNumber = 1 };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<SessionRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.SessionName.Should().Be("Back & Biceps");
    }

    [Fact]
    public async Task BatchCreate_ShouldReturnCreated_WhenBatchSucceeds()
    {
        // Arrange
        var dtos = new List<SessionCDTO>
        {
            new() { SessionName = "Day 1", DayNumber = 1, WorkoutPlanId = 1 },
            new() { SessionName = "Day 2", DayNumber = 2, WorkoutPlanId = 1 }
        };
        var rDtos = new List<SessionRDTO>
        {
            new() { Id = 1, SessionName = "Day 1", DayNumber = 1 },
            new() { Id = 2, SessionName = "Day 2", DayNumber = 2 }
        };

        _service.Setup(s => s.AddRangeAsync(dtos, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDtos);

        // Act
        var result = await _sut.BatchCreate(dtos, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<IEnumerable<SessionRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new SessionRDTO { Id = id, SessionName = "To Delete" };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<SessionRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Approve_ShouldReturnOk_WhenApprovalSucceeds()
    {
        // Arrange
        int id = 1;
        _service.Setup(s => s.ApproveAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Approve(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Contain("approved");
    }
}
