using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
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

public class SessionExercisesControllerTests
{
    private readonly Mock<ISessionExerciseService> _service;
    private readonly Mock<ILogger<SessionExercisesController>> _logger;
    private readonly SessionExercisesController _sut;

    public SessionExercisesControllerTests()
    {
        _service = new Mock<ISessionExerciseService>();
        _logger = new Mock<ILogger<SessionExercisesController>>();
        _sut = new SessionExercisesController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task BatchCreate_ShouldReturnCreated_WhenExercisesCreated()
    {
        // Arrange
        var dtos = new List<SessionExerciseCDTO>
        {
            new() { SessionId = 1, ExerciseName = "Bench Press", Sets = 4, Reps = 8, OrderIndex = 1 },
            new() { SessionId = 1, ExerciseName = "Incline Dumbbell Press", Sets = 3, Reps = 10, OrderIndex = 2 }
        };
        var rDtos = new List<SessionExerciseRDTO>
        {
            new() { Id = 1, SessionId = 1, ExerciseName = "Bench Press", Sets = 4, Reps = 8, OrderIndex = 1 },
            new() { Id = 2, SessionId = 1, ExerciseName = "Incline Dumbbell Press", Sets = 3, Reps = 10, OrderIndex = 2 }
        };

        _service.Setup(s => s.AddRangeAsync(dtos, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDtos);

        // Act
        var result = await _sut.BatchCreate(dtos, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<IEnumerable<SessionExerciseRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task BatchDelete_ShouldReturnOk_WhenExercisesDeleted()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };
        _service.Setup(s => s.DeleteRangeAsync(ids, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.BatchDelete(ids, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Contain("deleted");
    }
}
