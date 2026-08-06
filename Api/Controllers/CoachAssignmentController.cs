
using System.Threading;
using System.Threading.Tasks;
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

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoachAssignmentController(
    ICoachAssignmentService coachAssignmentService,
    ILogger<CoachAssignmentController> logger)
    : ControllerBase
{


    [HttpPost("get-assigned-members-for-coach")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetAssignedMembers([FromBody] GetAssignedMemberForCoachPagedReq req)
    {
        logger.LogInformation("Fetching all assigned members for coach");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned members for coach");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-assigned-coaches-for-member")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetAssignedCoaches([FromBody] GetAssignCoachForMemberPagedReq req)
    {
        logger.LogInformation("Fetching all assigned coaches for member");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned coaches for member");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-gym-coach-assignments")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
    public async Task<ActionResult<IEnumerable<CoachAssignmentRDTO>>> GetGymCoachAssignments([FromBody] PaginatedSearchReq searchReq)
    {
        logger.LogInformation("Fetching all assigned coaches for member");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(searchReq, false, CancellationToken.None);
        logger.LogInformation("Successfully fetched all assigned coaches for member");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("coach-assignments")]
    [EnableRateLimiting("UserRateLimiter")]

    public async Task<IActionResult> AssignCoach(
        [FromBody] CoachAssignmentCDTO dto,
        CancellationToken ct)
    {

        var result = await coachAssignmentService.AddAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, Result<CoachAssignmentRDTO>.Success(result));
    }
}
