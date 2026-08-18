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
    [HttpGet("{id:int}")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
    public async Task<ActionResult<Result<CoachAssignmentRDTO>>> GetById(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching coach assignment details for ID {Id}", id);
        CoachAssignmentRDTO? data = await coachAssignmentService.GetByIdDetailsAsync(id, cancellationToken);
        if (data is null)
        {
            logger.LogWarning("Coach assignment with ID {Id} not found", id);
            return NotFound(Result<CoachAssignmentRDTO>.Failure("NotFound", $"Coach assignment with ID {id} was not found."));
        }
        logger.LogInformation("Successfully fetched coach assignment details for ID {Id}", id);
        return Ok(Result<CoachAssignmentRDTO>.Success(data));
    }

    [HttpPost("get-assigned-members-for-coach")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach)]
    public async Task<ActionResult<Result<PaginatedRes<CoachAssignmentRDTO>>>> GetAssignedMembers(
        [FromBody] GetAssignedMemberForCoachPagedReq req,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all assigned members for coach");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, cancellationToken);
        logger.LogInformation("Successfully fetched all assigned members for coach");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-assigned-coaches-for-member")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager, GymRoleString.Coach, GymRoleString.Member)]
    public async Task<ActionResult<Result<PaginatedRes<CoachAssignmentRDTO>>>> GetAssignedCoaches(
        [FromBody] GetAssignCoachForMemberPagedReq req,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all assigned coaches for member");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(req, false, cancellationToken);
        logger.LogInformation("Successfully fetched all assigned coaches for member");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("get-gym-coach-assignments")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
    public async Task<ActionResult<Result<PaginatedRes<CoachAssignmentRDTO>>>> GetGymCoachAssignments(
        [FromBody] PaginatedSearchReq searchReq,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all assigned coaches for gym");
        PaginatedRes<CoachAssignmentRDTO> data = await coachAssignmentService.GetPageAsync(searchReq, false, cancellationToken);
        logger.LogInformation("Successfully fetched all assigned coaches for gym");
        return Ok(Result<PaginatedRes<CoachAssignmentRDTO>>.Success(data));
    }

    [HttpPost("coach-assignments")]
    [EnableRateLimiting("UserRateLimiter")]
    [GymAuthorize(GymRoleString.Owner, GymRoleString.Manager)]
    public async Task<ActionResult<Result<CoachAssignmentRDTO>>> AssignCoach(
        [FromBody] CoachAssignmentCDTO dto,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning coach {CoachId} to member {MemberId}", dto.CoachStaffId, dto.MemberId);
        CoachAssignmentRDTO result = await coachAssignmentService.AddAsync(dto, cancellationToken);
        logger.LogInformation("Successfully assigned coach {CoachId} to member {MemberId}", dto.CoachStaffId, dto.MemberId);
        return StatusCode(StatusCodes.Status201Created, Result<CoachAssignmentRDTO>.Success(result));
    }
}
