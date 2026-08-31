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

public class RevenuesControllerTests
{
    private readonly Mock<IRevenueService> _service;
    private readonly Mock<ILogger<RevenuesController>> _logger;
    private readonly RevenuesController _sut;

    public RevenuesControllerTests()
    {
        _service = new Mock<IRevenueService>();
        _logger = new Mock<ILogger<RevenuesController>>();
        _sut = new RevenuesController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new RevenueRDTO
        {
            Id = id,
            Amount = 150.0m,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Cash,
            RevenueDate = DateTime.UtcNow
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<RevenueRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.Amount.Should().Be(150.0m);
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<RevenueRDTO>
        {
            Items = new List<RevenueRDTO>
            {
                new RevenueRDTO { Id = 1, Amount = 150.0m, RevenueCategory = RevenueCategory.Membership }
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
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<RevenueRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new RevenueCDTO
        {
            Amount = 200.0m,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Instapay,
            RevenueDate = DateTime.UtcNow
        };
        var rDto = new RevenueRDTO { Id = 1, Amount = 200.0m, RevenueCategory = RevenueCategory.Membership };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<RevenueRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Amount.Should().Be(200.0m);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new RevenueUDTO
        {
            Amount = 250.0m,
            RevenueCategory = RevenueCategory.Membership,
            PaymentMethod = PaymentMethod.Instapay,
            RevenueDate = DateTime.UtcNow
        };
        var rDto = new RevenueRDTO { Id = id, Amount = 250.0m, RevenueCategory = RevenueCategory.Membership };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<RevenueRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Amount.Should().Be(250.0m);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new RevenueRDTO { Id = id, Amount = 150.0m };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<RevenueRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }
}
