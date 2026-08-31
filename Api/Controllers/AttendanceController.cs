using Api.Filters;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger) : ControllerBase
    {
        [HttpGet("{id:int}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach)]
        public async Task<ActionResult<Result<AttendanceLogItemRDTO>>> GetById(int id, CancellationToken ct)
        {
            logger.LogInformation("Retrieving attendance record by ID: {Id}", id);
            var result = await attendanceService.GetByIdDetailsAsync(id, ct);
            return Ok(Result<AttendanceLogItemRDTO>.Success(result));
        }

        [HttpPost("check-in")]
        [EnableRateLimiting("UserRateLimiter")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist)]
        public async Task<ActionResult<Result<AttendanceLogItemRDTO>>> CheckIn([FromBody] RecordCheckInCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Recording check-in for member {MemberId} in gym {GymId}", dto.MemberId, dto.GymId);
            var result = await attendanceService.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<AttendanceLogItemRDTO>.Success(result));
        }

        [HttpPost("history")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist)]
        public async Task<ActionResult<Result<PaginatedRes<AttendanceLogItemRDTO>>>> GetHistory([FromBody] PaginatedSearchReq req, CancellationToken ct)
        {
            logger.LogInformation("Retrieving full gym attendance history");
            var pagedData = await attendanceService.GetPageAsync(req, false, ct);
            return Ok(Result<PaginatedRes<AttendanceLogItemRDTO>>.Success(pagedData));
        }

        [HttpPost("member-history")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<Result<PaginatedRes<AttendanceLogItemRDTO>>>> GetMemberAttendanceHistory([FromBody] MemberAttendanceHistoryPagedReq req, CancellationToken ct)
        {
            logger.LogInformation("Retrieving member attendance history for member {MemberId}", req.MemberId);
            var result = await attendanceService.GetPageAsync(req, false, ct);
            return Ok(Result<PaginatedRes<AttendanceLogItemRDTO>>.Success(result));
        }
    }
}
