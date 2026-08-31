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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class AttendanceControllerTests
{
    private readonly Mock<IAttendanceService> _service;
    private readonly Mock<ILogger<AttendanceController>> _logger;
    private readonly AttendanceController _sut;

    public AttendanceControllerTests()
    {
        _service = new Mock<IAttendanceService>();
        _logger = new Mock<ILogger<AttendanceController>>();
        _sut = new AttendanceController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task CheckIn_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        int gymId = 1;
        var dto = new RecordCheckInCDTO { MemberId = 12, GymId = gymId };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttendanceLogItemRDTO());

        // Act
        var result = await _sut.CheckIn(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
        var response = objectResult.Value.Should().BeAssignableTo<Result<AttendanceLogItemRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 5;
        var rdto = new AttendanceLogItemRDTO { Id = id, MemberId = 12, MemberFullName = "John Doe" };
        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rdto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<AttendanceLogItemRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
    }
}
