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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class MemberWorkoutPlansControllerTests
{
    private readonly Mock<IMemberWorkoutPlanService> _service;
    private readonly Mock<ILogger<MemberWorkoutPlansController>> _logger;
    private readonly MemberWorkoutPlansController _sut;

    public MemberWorkoutPlansControllerTests()
    {
        _service = new Mock<IMemberWorkoutPlanService>();
        _logger = new Mock<ILogger<MemberWorkoutPlansController>>();
        _sut = new MemberWorkoutPlansController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new MemberWorkoutPlanRDTO
        {
            Id = id,
            WorkoutPlanId = 2,
            WorkoutPlanName = "Strength Plan",
            MemberId = 5,
            MemberName = "John Doe",
            Status = MemberWorkoutPlanStatus.Active,
            StartDate = DateTime.UtcNow
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MemberWorkoutPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.MemberName.Should().Be("John Doe");
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<MemberWorkoutPlanRDTO>
        {
            Items = new List<MemberWorkoutPlanRDTO>
            {
                new() { Id = 1, MemberName = "Jane Doe", Status = MemberWorkoutPlanStatus.Active }
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
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<MemberWorkoutPlanRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new MemberWorkoutPlanCDTO { WorkoutPlanId = 1, MemberId = 2, StartDate = DateTime.UtcNow };
        var rDto = new MemberWorkoutPlanRDTO { Id = 1, WorkoutPlanId = 1, MemberId = 2 };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<MemberWorkoutPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.WorkoutPlanId.Should().Be(1);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new MemberWorkoutPlanUDTO { WorkoutPlanId = 1, MemberId = 2, StartDate = DateTime.UtcNow, Status = MemberWorkoutPlanStatus.Completed };
        var rDto = new MemberWorkoutPlanRDTO { Id = id, Status = MemberWorkoutPlanStatus.Completed };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MemberWorkoutPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Status.Should().Be(MemberWorkoutPlanStatus.Completed);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new MemberWorkoutPlanRDTO { Id = id };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MemberWorkoutPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CancelAssignment_ShouldReturnOk_WhenCancellationSucceeds()
    {
        // Arrange
        int id = 1;
        _service.Setup(s => s.CancelAssignmentAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CancelAssignment(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<string>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Contain("cancelled");
    }
}
