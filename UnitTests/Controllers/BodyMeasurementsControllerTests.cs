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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers;

public class BodyMeasurementsControllerTests
{
    private readonly Mock<IBodyMeasurementService> _service;
    private readonly Mock<ILogger<BodyMeasurementsController>> _logger;
    private readonly BodyMeasurementsController _sut;

    public BodyMeasurementsControllerTests()
    {
        _service = new Mock<IBodyMeasurementService>();
        _logger = new Mock<ILogger<BodyMeasurementsController>>();
        _sut = new BodyMeasurementsController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new BodyMeasurementRDTO
        {
            Id = id,
            WeightKg = 75.5m,
            HeightCm = 180m,
            CreatedById = 10,
            CreatedOn = DateTime.UtcNow
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<BodyMeasurementRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.WeightKg.Should().Be(75.5m);
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<BodyMeasurementRDTO>
        {
            Items = new List<BodyMeasurementRDTO>
            {
                new BodyMeasurementRDTO { Id = 1, WeightKg = 75.5m, HeightCm = 180m }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _service.Setup(s => s.GetPageAsync(req, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResult);

        // Act
        var result = await _sut.GetPaged(req);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<BodyMeasurementRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new BodyMeasurementCDTO { WeightKg = 80.0m, HeightCm = 175m, Notes = "Initial assessment" };
        var rDto = new BodyMeasurementRDTO { Id = 1, WeightKg = 80.0m, HeightCm = 175m, Notes = "Initial assessment" };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<BodyMeasurementRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.WeightKg.Should().Be(80.0m);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new BodyMeasurementUDTO { WeightKg = 82.0m, HeightCm = 175m };
        var rDto = new BodyMeasurementRDTO { Id = id, WeightKg = 82.0m, HeightCm = 175m };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<BodyMeasurementRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.WeightKg.Should().Be(82.0m);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new BodyMeasurementRDTO { Id = id, WeightKg = 75.0m };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<BodyMeasurementRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }
}
