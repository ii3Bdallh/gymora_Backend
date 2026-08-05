using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
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

public class TraineeCoachControllerTests
{
    private readonly Mock<ICoachAssignmentService> _service;
    private readonly Mock<ILogger<TraineeCoachController>> _logger;
    private readonly TraineeCoachController _sut;

    public TraineeCoachControllerTests()
    {
        _service = new Mock<ICoachAssignmentService>();
        _logger = new Mock<ILogger<TraineeCoachController>>();
        _sut = new TraineeCoachController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetAssignedTrainees_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        int gymId = 1;
        var req = new GetAssignedMemberForCoachPagedReq { PageNumber = 1, PageSize = 10, CoachId = 5, GymId = gymId };
        var listResult = new PaginatedRes<CoachAssignmentRDTO>
        {
            Items = new List<CoachAssignmentRDTO>
            {
                new CoachAssignmentRDTO
                {
                    Id = 1,
                    GymId = gymId,
                    MemberId = 12,
                    CoachStaffId = 5,
                    AssignedAt = DateTime.UtcNow,
                    Member = new GymPersonRDTO { Name = "John Doe" }
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _service.Setup(s => s.GetPageAsync(req, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResult);

        // Act
        var result = await _sut.GetAssignedTrainees(gymId, req, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<CoachAssignmentRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
        response.Data.Items.First().Member!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetAssignedTrainees_ShouldReturnUnprocessableEntity_WhenParametersAreInvalid()
    {
        // Arrange
        int gymId = 1;
        var req = new GetAssignedMemberForCoachPagedReq { PageNumber = 0, PageSize = 10, CoachId = 5, GymId = gymId };

        // Act
        var result = await _sut.GetAssignedTrainees(gymId, req, CancellationToken.None);

        // Assert
        var badResult = result.Should().BeOfType<ObjectResult>().Subject;
        badResult.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task AssignCoach_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        int gymId = 1;
        var dto = new CoachAssignmentCDTO { MemberId = 12, CoachStaffId = 5, GymId = gymId };
        var rDto = new CoachAssignmentRDTO
        {
            Id = 1,
            GymId = gymId,
            MemberId = 12,
            CoachStaffId = 5,
            AssignedAt = DateTime.UtcNow,
            CoachStaff = new GymPersonRDTO { Name = "Coach Carter" }
        };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.AssignCoach(gymId, dto, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
        var response = objectResult.Value.Should().BeAssignableTo<Result<CoachAssignmentRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.CoachStaff!.Name.Should().Be("Coach Carter");
    }
}
