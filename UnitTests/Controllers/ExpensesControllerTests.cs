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

public class ExpensesControllerTests
{
    private readonly Mock<IExpenseService> _service;
    private readonly Mock<ILogger<ExpensesController>> _logger;
    private readonly ExpensesController _sut;

    public ExpensesControllerTests()
    {
        _service = new Mock<IExpenseService>();
        _logger = new Mock<ILogger<ExpensesController>>();
        _sut = new ExpensesController(_service.Object, _logger.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRecordExists()
    {
        // Arrange
        int id = 1;
        var rDto = new ExpenseRDTO
        {
            Id = id,
            Amount = 300.0m,
            ExpenseCategory = ExpenseCategory.Rent,
            PaymentMethod = PaymentMethod.BankTransfer,
            ExpenseDate = DateTime.UtcNow
        };

        _service.Setup(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.GetById(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExpenseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.Amount.Should().Be(300.0m);
        _service.Verify(s => s.GetByIdDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var req = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var listResult = new PaginatedRes<ExpenseRDTO>
        {
            Items = new List<ExpenseRDTO>
            {
                new ExpenseRDTO { Id = 1, Amount = 300.0m, ExpenseCategory = ExpenseCategory.Rent }
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
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<ExpenseRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = new ExpenseCDTO
        {
            Amount = 400.0m,
            ExpenseCategory = ExpenseCategory.Equipment,
            PaymentMethod = PaymentMethod.Instapay,
            ExpenseDate = DateTime.UtcNow
        };
        var rDto = new ExpenseRDTO { Id = 1, Amount = 400.0m, ExpenseCategory = ExpenseCategory.Equipment };

        _service.Setup(s => s.AddAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Create(dto, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeAssignableTo<Result<ExpenseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Amount.Should().Be(400.0m);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        int id = 1;
        var dto = new ExpenseUDTO
        {
            Amount = 450.0m,
            ExpenseCategory = ExpenseCategory.Equipment,
            PaymentMethod = PaymentMethod.Instapay,
            ExpenseDate = DateTime.UtcNow
        };
        var rDto = new ExpenseRDTO { Id = id, Amount = 450.0m, ExpenseCategory = ExpenseCategory.Equipment };

        _service.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Update(id, dto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExpenseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Amount.Should().Be(450.0m);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
    {
        // Arrange
        int id = 1;
        var rDto = new ExpenseRDTO { Id = id, Amount = 300.0m };

        _service.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        // Act
        var result = await _sut.Delete(id, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExpenseRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }
}
