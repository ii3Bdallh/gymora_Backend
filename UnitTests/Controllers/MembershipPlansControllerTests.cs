using Api.Controllers;
using Application.DTO;
using Application.DTO.Exceptions;
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

public class MembershipPlansControllerTests
{
    private readonly Mock<IMembershipPlanService> _service;
    private readonly Mock<ILogger<MembershipPlansController>> _logger;
    private readonly MembershipPlansController _sut;

    public MembershipPlansControllerTests()
    {
        _service = new Mock<IMembershipPlanService>();
        _logger = new Mock<ILogger<MembershipPlansController>>();
        _sut = new MembershipPlansController(_service.Object, _logger.Object);
    }

    #region GetPaged

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenDataExists()
    {
        var searchReq = new PaginatedSearchReq { PageNumber = 1, PageSize = 10 };
        var pageResult = new PaginatedRes<MembershipPlanRDTO>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<MembershipPlanRDTO>
            {
                new() { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m }
            }
        };

        _service.Setup(s => s.GetPageAsync(searchReq, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var result = await _sut.GetPaged(searchReq);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<PaginatedRes<MembershipPlanRDTO>>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("SearchTerm", "Invalid search term");
        var searchReq = new PaginatedSearchReq();

        var result = await _sut.GetPaged(searchReq);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenPlanExists()
    {
        var plan = new MembershipPlanRDTO
        {
            Id = 1,
            Name = "Monthly Gold",
            DurationDays = 30,
            Price = 100m
        };

        _service.Setup(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var result = await _sut.GetById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MembershipPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Name.Should().Be("Monthly Gold");
        _service.Verify(s => s.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldThrowNotFoundException_WhenPlanNotFound()
    {
        _service.Setup(s => s.GetByIdDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("MembershipPlan with ID 999 was not found."));

        var act = async () => await _sut.GetById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDataIsValid()
    {
        var cdto = new MembershipPlanCDTO
        {
            Name = "Monthly Gold",
            Description = "Full Gym Access",
            DurationDays = 30,
            Price = 100m
        };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Monthly Gold", DurationDays = 30, Price = 100m };

        _service.Setup(s => s.AddAsync(cdto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.Create(cdto);

        var createdResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = createdResult.Value.Should().BeAssignableTo<Result<MembershipPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.Create(new MembershipPlanCDTO { Name = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldReturnOk_WhenDataIsValid()
    {
        var udto = new MembershipPlanUDTO
        {
            Name = "Updated Gold",
            DurationDays = 60,
            Price = 180m
        };
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Updated Gold", DurationDays = 60, Price = 180m };

        _service.Setup(s => s.UpdateAsync(1, udto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.Update(1, udto);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MembershipPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.Update(1, new MembershipPlanUDTO { Name = null! });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenPlanExists()
    {
        var rDto = new MembershipPlanRDTO { Id = 1, Name = "Monthly Gold" };

        _service.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rDto);

        var result = await _sut.Delete(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<MembershipPlanRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
    }

    #endregion
}
