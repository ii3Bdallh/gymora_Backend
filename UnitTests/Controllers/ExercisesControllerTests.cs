using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
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

public class ExercisesControllerTests
{
    private readonly Mock<IExerciseService> _service;
    private readonly Mock<ILogger<ExercisesController>> _logger;
    private readonly ExercisesController _sut;

    public ExercisesControllerTests()
    {
        _service = new Mock<IExerciseService>();
        _logger = new Mock<ILogger<ExercisesController>>();
        _sut = new ExercisesController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new ExerciseRDTO
        {
            Id = id,
            Name = "Push Up",
            PrimaryMuscle = MuscleGroup.Chest,
            DifficultyLevel = DifficultyLevel.Beginner,
            IsApproved = true
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExerciseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.Name.Should().Be("Push Up");
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<ExerciseRDTO>
        {
            Items = new List<ExerciseRDTO>
            {
                new() { Id = 1, Name = "Bench Press", PrimaryMuscle = MuscleGroup.Chest }
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
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<ExerciseRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new ExerciseCDTO { Name = "Pull Up", PrimaryMuscle = MuscleGroup.Back };
        var rDto = new ExerciseRDTO { Id = 1, Name = "Pull Up", PrimaryMuscle = MuscleGroup.Back };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<ExerciseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Pull Up");
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new ExerciseUDTO { Name = "Updated Name", PrimaryMuscle = MuscleGroup.Back };
        var rDto = new ExerciseRDTO { Id = id, Name = "Updated Name", PrimaryMuscle = MuscleGroup.Back };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExerciseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new ExerciseRDTO { Id = id, Name = "Push Up" };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExerciseRDTO>>().Subject;
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
