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
    [Route("api/v1/gyms/{gymId}/finances")]
    [Authorize]
    public class FinancialReportsController(IFinancialReportService service, ILogger<FinancialReportsController> logger) : ControllerBase
    {
        [HttpGet("reports/revenue")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<RevenueReportRDTO>>> GetRevenueReport(
            int gymId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken ct)
        {
            logger.LogInformation("Generating revenue report for gym {GymId} from {FromDate} to {ToDate}", gymId, fromDate, toDate);

            // Validation checks
            if (fromDate > DateTime.UtcNow)
            {
                return UnprocessableEntity(Result<RevenueReportRDTO>.Failure("VALIDATION_ERROR", "FromDate cannot be in the future."));
            }
            if (toDate < fromDate)
            {
                return UnprocessableEntity(Result<RevenueReportRDTO>.Failure("VALIDATION_ERROR", "ToDate must be greater than or equal to FromDate."));
            }

            var report = await service.GetRevenueReportAsync(gymId, fromDate, toDate, ct);
            return Ok(Result<RevenueReportRDTO>.Success(report));
        }

        [HttpGet("reports/expense")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
        public async Task<ActionResult<Result<ExpenseReportRDTO>>> GetExpenseReport(
            int gymId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken ct)
        {
            logger.LogInformation("Generating expense report for gym {GymId} from {FromDate} to {ToDate}", gymId, fromDate, toDate);

            // Validation checks
            if (fromDate > DateTime.UtcNow)
            {
                return UnprocessableEntity(Result<ExpenseReportRDTO>.Failure("VALIDATION_ERROR", "FromDate cannot be in the future."));
            }
            if (toDate < fromDate)
            {
                return UnprocessableEntity(Result<ExpenseReportRDTO>.Failure("VALIDATION_ERROR", "ToDate must be greater than or equal to FromDate."));
            }

            var report = await service.GetExpenseReportAsync(gymId, fromDate, toDate, ct);
            return Ok(Result<ExpenseReportRDTO>.Success(report));
        }
    }
}
