using Api.Controllers;
using Application.DTO;
using Application.DTO.Model;
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

public class ReportsControllerTests
{
    private readonly Mock<IReportService> _reportService;
    private readonly Mock<ILogger<ReportsController>> _logger;
    private readonly ReportsController _sut;

    public ReportsControllerTests()
    {
        _reportService = new Mock<IReportService>();
        _logger = new Mock<ILogger<ReportsController>>();
        _sut = new ReportsController(_reportService.Object, _logger.Object);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        int gymId = 1;
        var dashboardData = new GymAttendanceDashboardRDTO
        {
            GymId = gymId,
            Stats = new AttendanceDashboardStatsRDTO(5, 3, 20, 2),
            RecentEntries = new List<RecentCheckInItemRDTO>()
        };

        _reportService.Setup(s => s.GetDashboardAsync(gymId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboardData);

        // Act
        var result = await _sut.GetDashboard(gymId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<GymAttendanceDashboardRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.Stats.TodayCheckInsCount.Should().Be(5);
    }

    [Fact]
    public async Task GetRevenueReport_ShouldReturnOk_WhenDatesAreValid()
    {
        // Arrange
        int gymId = 1;
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        var reportData = new RevenueReportRDTO { TotalRevenue = 1500m };

        _reportService.Setup(s => s.GetRevenueReportAsync(gymId, fromDate, toDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _sut.GetRevenueReport(gymId, fromDate, toDate, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<RevenueReportRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.TotalRevenue.Should().Be(1500m);
    }

    [Fact]
    public async Task GetExpenseReport_ShouldReturnOk_WhenDatesAreValid()
    {
        // Arrange
        int gymId = 1;
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        var reportData = new ExpenseReportRDTO { TotalExpense = 500m };

        _reportService.Setup(s => s.GetExpenseReportAsync(gymId, fromDate, toDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _sut.GetExpenseReport(gymId, fromDate, toDate, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<ExpenseReportRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.TotalExpense.Should().Be(500m);
    }

    [Fact]
    public async Task GetAttendanceReport_ShouldReturnOk_WhenDatesAreValid()
    {
        // Arrange
        int gymId = 1;
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        var reportData = new AttendanceReportRDTO { TotalCheckIns = 42 };

        _reportService.Setup(s => s.GetAttendanceReportAsync(gymId, fromDate, toDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportData);

        // Act
        var result = await _sut.GetAttendanceReport(gymId, fromDate, toDate, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<Result<AttendanceReportRDTO>>().Subject;
        response.IsSuccess.Should().BeTrue();
        response.Data!.TotalCheckIns.Should().Be(42);
    }
}
