using Api.Filters;
using Application.DTO;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger) : ControllerBase
    {
        [HttpGet("dashboard/{gymId}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach)]
        public async Task<ActionResult<Result<GymAttendanceDashboardRDTO>>> GetDashboard(int gymId, CancellationToken ct)
        {
            logger.LogInformation("Retrieving attendance dashboard for gym: {GymId}", gymId);
            var result = await reportService.GetDashboardAsync(gymId, ct);
            return Ok(Result<GymAttendanceDashboardRDTO>.Success(result));
        }

        [HttpGet("revenue/{gymId}")]

        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueReportRDTO>>> GetRevenueReport(
            int gymId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken ct)
        {
            logger.LogInformation("Generating revenue report for gym {GymId} from {FromDate} to {ToDate}", gymId, fromDate, toDate);

            if (fromDate > DateTime.UtcNow)
            {
                return UnprocessableEntity(Result<RevenueReportRDTO>.Failure("VALIDATION_ERROR", "FromDate cannot be in the future."));
            }
            if (toDate < fromDate)
            {
                return UnprocessableEntity(Result<RevenueReportRDTO>.Failure("VALIDATION_ERROR", "ToDate must be greater than or equal to FromDate."));
            }

            var report = await reportService.GetRevenueReportAsync(gymId, fromDate, toDate, ct);
            return Ok(Result<RevenueReportRDTO>.Success(report));
        }

        [HttpGet("expense/{gymId}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseReportRDTO>>> GetExpenseReport(
            int gymId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken ct)
        {
            logger.LogInformation("Generating expense report for gym {GymId} from {FromDate} to {ToDate}", gymId, fromDate, toDate);

            if (fromDate > DateTime.UtcNow)
            {
                return UnprocessableEntity(Result<ExpenseReportRDTO>.Failure("VALIDATION_ERROR", "FromDate cannot be in the future."));
            }
            if (toDate < fromDate)
            {
                return UnprocessableEntity(Result<ExpenseReportRDTO>.Failure("VALIDATION_ERROR", "ToDate must be greater than or equal to FromDate."));
            }

            var report = await reportService.GetExpenseReportAsync(gymId, fromDate, toDate, ct);
            return Ok(Result<ExpenseReportRDTO>.Success(report));
        }

        [HttpGet("attendance/{gymId}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach)]
        public async Task<ActionResult<Result<AttendanceReportRDTO>>> GetAttendanceReport(
            int gymId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken ct)
        {
            logger.LogInformation("Generating attendance report for gym {GymId} from {FromDate} to {ToDate}", gymId, fromDate, toDate);

            if (fromDate > DateTime.UtcNow)
            {
                return UnprocessableEntity(Result<AttendanceReportRDTO>.Failure("VALIDATION_ERROR", "FromDate cannot be in the future."));
            }
            if (toDate < fromDate)
            {
                return UnprocessableEntity(Result<AttendanceReportRDTO>.Failure("VALIDATION_ERROR", "ToDate must be greater than or equal to FromDate."));
            }

            var report = await reportService.GetAttendanceReportAsync(gymId, fromDate, toDate, ct);
            return Ok(Result<AttendanceReportRDTO>.Success(report));
        }
    }
}
