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

    public class AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger) : ControllerBase
    {
        [HttpGet("dashboard/{gymId}")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach)]
        public async Task<ActionResult<GymAttendanceDashboardRDTO>> GetDashboard(int gymId, CancellationToken ct)
        {
            logger.LogInformation("Retrieving attendance dashboard for gym: {GymId}", gymId);
            var result = await attendanceService.GetDashboardAsync(gymId, ct);
            return Ok(Result<GymAttendanceDashboardRDTO>.Success(result));
        }

        [HttpPost("check-in")]
        [EnableRateLimiting("UserRateLimiter")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist)]
        public async Task<IActionResult> CheckIn([FromBody] RecordCheckInCDTO dto, CancellationToken ct)
        {
            logger.LogInformation("Recording check-in for member {MemberId} in gym {GymId}", dto.MemberId, dto.GymId);
            await attendanceService.AddAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, Result<object>.Success(new { Message = "Check-in recorded successfully." }));
        }

        // [HttpGet("gyms/{gymId}/attendance/search-members")]
        // [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach)]
        // public async Task<ActionResult<CheckInMemberListRDTO>> SearchMembers(int gymId, [FromQuery] string searchTerm, CancellationToken ct)
        // {
        //     logger.LogInformation("Searching members for check-in in gym {GymId} with search term: {SearchTerm}", gymId, searchTerm);
        //     var result = await attendanceService.SearchMembersForCheckInAsync(gymId, searchTerm, ct);
        //     return Ok(Result<CheckInMemberListRDTO>.Success(result));
        // }

        [HttpPost("history")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist)]
        public async Task<ActionResult<PaginatedRes<AttendanceLogItemRDTO>>> GetHistory([FromBody] PaginatedSearchReq req, CancellationToken ct)
        {
            logger.LogInformation("Retrieving full gym attendance history");
            var pagedData = await attendanceService.GetPageAsync(req, false, ct);
            return Ok(Result<PaginatedRes<AttendanceLogItemRDTO>>.Success(pagedData));
        }

        // [HttpPost("gyms/{gymId}/members/{memberId}/attendance")]
        // [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach, GymRoleString.Member)]
        // public async Task<ActionResult<PaginatedRes<AttendanceLogItemRDTO>>> GetMemberAttendance(int gymId, int memberId, [FromBody] MemberAttendanceHistoryPagedReq req, CancellationToken ct)
        // {
        //     logger.LogInformation("Retrieving member attendance for member {MemberId} in gym {GymId}", memberId, gymId);
        //     req.MemberId = memberId;
        //     var result = await attendanceService.GetMemberAttendanceAsync(gymId, req, ct);
        //     return Ok(Result<PaginatedRes<AttendanceLogItemRDTO>>.Success(result));
        // }

        [HttpPost("member-history")]
        [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Receptionist, GymRoleString.Coach, GymRoleString.Member)]
        public async Task<ActionResult<PaginatedRes<AttendanceLogItemRDTO>>> GetMemberAttendanceHistory([FromBody] MemberAttendanceHistoryPagedReq req, CancellationToken ct)
        {
            logger.LogInformation("Retrieving member attendance history for member {MemberId}", req.MemberId);
            var result = await attendanceService.GetPageAsync(req, false, ct);
            return Ok(Result<PaginatedRes<AttendanceLogItemRDTO>>.Success(result));
        }
    }
}
